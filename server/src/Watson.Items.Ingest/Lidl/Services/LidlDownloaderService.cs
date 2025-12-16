using Bureau.Core;
using Bureau.Core.FileSignature;
using HtmlAgilityPack;
using Niles.Data;
using Niles.Etl.Configurations;
using Niles.Etl.Extract;
using Niles.Etl.Jobs;
using Niles.IO;

namespace Niles.Etl.Lidl.Services
{
    public partial class LidlDownloaderService : IDownloader
    {
        private readonly HttpClient _client;
        private readonly IArtifactRepository _artifactRepository;

        private sealed record RunContext(
            JobInfo Job,
            RetailerEtlOptions Options,
            Func<JobArtifact, CancellationToken, Task> OnDownloadFinishedAsync
        );

        public LidlDownloaderService(
            HttpClient httpClient,
            IArtifactRepository artifactRepository)
        {
            _client = httpClient;
            _artifactRepository = artifactRepository;
        }
        /// <summary>
        /// Discover ZIP URLs, skip already-processed URLs via IProcessedStore, download each ZIP
        /// to targetDir, and invoke onZipReady(path) as soon as each ZIP is saved.
        /// Leaves DownloadAndExtractPricesAsync intact for existing flows.
        /// </summary>
        public async Task<Result> DownloadAsync(JobInfo job, RetailerEtlOptions options, Func<JobArtifact, CancellationToken, Task> onDownloadReadyAsync,
            CancellationToken cancellationToken = default)
        {
            RunContext context = new(job, options, onDownloadReadyAsync);

            Directory.CreateDirectory(context.Options.InboundFolder);

            context.Job.ReportProgress(new JobProgress { Message = $"Downloader (ZIP only). ExtractUrl={context.Options.ExtractUrl}, BaseUrl={context.Options.BaseUrl}, TargetDir={context.Options.InboundFolder}, Force={context.Options.Force}" });

            string htmlContent = await _client.GetStringAsync(context.Options.ExtractUrl, cancellationToken);
            List<JobArtifact> urls = ExtractDownloadUrls(htmlContent, context.Options.BaseUrl);

            context.Job.ReportProgress(new JobProgress { Found = urls.Count, Message = "Discovered download URLs." });

            List<Task<ArtifactStatus>> tasks = new(urls.Count);

            foreach (JobArtifact url in urls)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                tasks.Add(HandleDownloadUrlAsync(context, url, cancellationToken));
            }
            ArtifactStatus[] results = await Task.WhenAll(tasks);

            int downloaded = 0, skipped = 0;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] == ArtifactStatus.Skipped)
                {
                    skipped++;
                }
                else if (results[i] == ArtifactStatus.Completed)
                {
                    downloaded++;
                }
            }
            job.ReportProgress(new JobProgress { Found = urls.Count, Message = $"Download phase finished. Downloaded={downloaded}, Skipped={skipped}." });
            return true;
        }

        private static List<JobArtifact> ExtractDownloadUrls(string htmlContent, string baseUrl)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(htmlContent);

            HtmlNodeCollection? linkNodes = doc.DocumentNode.SelectNodes("//a[contains(text(), 'ovdje')]");
            List<JobArtifact> urls = new List<JobArtifact>(linkNodes?.Count ?? 0);
            if (linkNodes != null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    string href = linkNode.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(href)) continue;

                    if (href.StartsWith('/'))
                    {
                        href = $"{baseUrl}{href}";
                    }
                    else if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        href = $"{baseUrl}/{href.TrimStart('/')}";
                    }
                    urls.Add(new JobArtifact(href, IO.ArtifactType.Url));
                }
            }
            return urls;
        }

        private async Task<ArtifactStatus> HandleDownloadUrlAsync(RunContext context, JobArtifact url, CancellationToken cancellationToken)
        {
            //TODO some kind of scope for artifact repo string key = url.Identity.ToCanonicalUrl();
            ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(context.Job.Id, url, cancellationToken);
            if (!context.Options.Force && status.IsProcessed())
            {
                context.Job.ReportProgress(new JobProgress { Item = url, Skipped = 1, Message = "Entry already completed. Skipping." });
                return ArtifactStatus.Skipped;
            }

            Result<JobArtifact> savedPathResult = await DownloadZipAsync(url.Identity, context.Options.InboundFolder, cancellationToken);
            if (savedPathResult.IsError)
            {
                await _artifactRepository.MarkFailedAsync(url, savedPathResult.Error.ErrorMessage, cancellationToken);
                context.Job.ReportProgress(new JobProgress
                {
                    Item = url,
                    Error = savedPathResult.Error.ErrorMessage,
                    Message = "Download failed."
                });
                return ArtifactStatus.Failed;
            }
            await _artifactRepository.MarkCompletedAsync(url, cancellationToken);
            await context.OnDownloadFinishedAsync(savedPathResult.Value, cancellationToken);
            return ArtifactStatus.Completed;
        }

        private async Task<Result<JobArtifact>> DownloadZipAsync(string url, string targetDir, CancellationToken cancellationToken)
        {
            if (!url.Is<ZipFileSignature>(out string fileName))
            {
                return new ResultError($"URL does not look like a ZIP: {url}");
            }

            byte[] fileBytes = await _client.GetByteArrayAsync(url, cancellationToken);
            if (!fileBytes.Is<ZipFileSignature>())
            {
                return new ResultError($"Downloaded file is not a ZIP: {url}");
            }

            Directory.CreateDirectory(targetDir);
            string path = Path.Combine(targetDir, fileName);
            await File.WriteAllBytesAsync(path, fileBytes, cancellationToken);
            return new JobArtifact(path, IO.ArtifactType.File);
        }
    }
}
