using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Core.FileSignature;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Niles.Etl.Abstractions.Configurations;
using Niles.Etl.Jobs;
using System.IO.Compression;

namespace Niles.Etl.Lidl.Services
{
    public class DownloaderService
    {
        private readonly ILogger<DownloaderService> _logger;
        private readonly DownloaderOptions _options;
        private readonly HttpClient _client;
        private readonly IProcessedStore _processed;

        public DownloaderService(
            ILogger<DownloaderService> logger,
            IOptions<DownloaderOptions> options,
            HttpClient httpClient,
            IProcessedStore processedStore)
        {
            _logger = logger;
            _options = options.Value;
            _client = httpClient;
            _processed = processedStore;
        }

        public async Task DownloadAndExtractPricesAsync(
            IJobReporter reporter,
            string jobId,
            Niles.Etl.Lidl.Models.DownloaderJobArgs args,
            CancellationToken cancellationToken = default)
        {
            string mainUrl = args.MainUrlOverride ?? _options.MainUrl;
            string baseUrl = args.BaseUrlOverride ?? _options.BaseUrl;
            string targetDir = args.TargetDirectory ?? _options.TempDirectory;
            string scope = GetScope(baseUrl);

            Directory.CreateDirectory(targetDir);

            _logger.LogDebugInfo("Downloader starting. MainUrl={MainUrl}, BaseUrl={BaseUrl}, TargetDir={TargetDir}, Force={Force}",
                mainUrl, baseUrl, targetDir, args.Force);

            string htmlContent = await _client.GetStringAsync(mainUrl, cancellationToken);
            List<string> urls = ExtractDownloadUrls(htmlContent, baseUrl);

            reporter.Report(jobId, new JobProgress
            {
                TotalFound = urls.Count,
                Message = "Discovered download URLs."
            });

            int processed = 0;
            int skipped = 0;

            foreach (string url in urls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                reporter.Report(jobId, new JobProgress
                {
                    CurrentItem = url,
                    Processed = processed,
                    Skipped = skipped,
                    Message = "Processing URL..."
                });

                string key = url.CanonicalizeUrl();

                if (!args.Force && _processed.IsProcessed(JobType.Downloader, key, scope))
                {
                    skipped++;
                    reporter.Report(jobId, new JobProgress
                    {
                        CurrentItem = url,
                        Processed = processed,
                        Skipped = skipped,
                        Message = "Already processed. Skipping."
                    });
                    continue;
                }

                Result result = await DownloadAndExtractFileAsync(url, targetDir, cancellationToken);
                if (result.IsSuccess)
                {
                    processed++;
                    _processed.MarkProcessed(JobType.Downloader, key, scope);
                    reporter.Report(jobId, new JobProgress
                    {
                        CurrentItem = url,
                        Processed = processed,
                        Skipped = skipped,
                        Message = "Extracted successfully."
                    });
                }
                else
                {
                    reporter.Report(jobId, new JobProgress
                    {
                        CurrentItem = url,
                        Error = result.Error.ErrorMessage,
                        Message = "Extraction failed."
                    });
                }
            }

            reporter.Report(jobId, new JobProgress
            {
                Message = $"Done. Processed={processed}, Skipped={skipped}."
            });
        }

        private static string GetScope(string baseUrl)
        {
            try
            {
                Uri uri = new Uri(baseUrl);
                return uri.Host; // "tvrtka.lidl.hr"
            }
            catch
            {
                return "default";
            }
        }

        private List<string> ExtractDownloadUrls(string htmlContent, string baseUrl)
        {
            List<string> urls = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            HtmlNodeCollection? linkNodes = doc.DocumentNode.SelectNodes("//a[contains(text(), 'ovdje')]");
            if (linkNodes != null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    string href = linkNode.GetAttributeValue("href", "");
                    if (String.IsNullOrEmpty(href)) continue;

                    if (href.StartsWith("/"))
                    {
                        href = $"{baseUrl}{href}";
                    }
                    else if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        href = $"{baseUrl}/{href.TrimStart('/')}";
                    }

                    urls.Add(href);
                }
            }

            return urls;
        }

        private async Task<Result> DownloadAndExtractFileAsync(string url, string targetDir, CancellationToken cancellationToken = default)
        {
            if (!url.Is<ZipFileSignature>(out string fileName))
            {
                return new ResultError($"File name not good: {fileName}");
            }

            byte[] fileBytes = await _client.GetByteArrayAsync(url, cancellationToken);
            if (!fileBytes.Is<ZipFileSignature>())
            {
                return new ResultError($"File is not of expected type or is corrupted: {fileName}");
            }

            using (MemoryStream ms = new MemoryStream(fileBytes, writable: false))
            using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue;

                    string destinationPath = GetSafeExtractPath(targetDir, entry.FullName);
                    string? directory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (Stream entryStream = entry.Open())
                    using (FileStream outStream = File.Create(destinationPath))
                    {
                        await entryStream.CopyToAsync(outStream, cancellationToken);
                    }
                }
            }

            _logger.LogDebugInfo("Extracted ZIP to {TargetDir}", targetDir);
            return true;
        }

        private static string GetSafeExtractPath(string targetDir, string entryName)
        {
            string fullPath = Path.GetFullPath(Path.Combine(targetDir, entryName));
            string targetFull = Path.GetFullPath(targetDir);
            if (!fullPath.StartsWith(targetFull, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsafe path in zip entry: {entryName}");
            }
            return fullPath;
        }
    }

}
