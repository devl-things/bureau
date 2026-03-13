using Bureau.Core;
using Bureau.Core.FileSignature;
using Niles.Data;
using Niles.Etl.Configurations;
using Niles.Etl.Extract;
using Niles.Etl.Jobs;
using Niles.Etl.Lidl.Models;
using Niles.Etl.Models;
using Niles.IO;
using Niles.Models;
using System.IO.Compression;
using System.Text;

namespace Niles.Etl.Lidl.Services
{
    public class ExtractorService
    {
        private readonly IArtifactRepository _artifactRepository;
        private readonly ZipManager _zipManager;

        private readonly IFileNameParser _fileNameParser;
        private readonly CsvExtractorService _csv;
        private readonly RowTransformerService _transform;
        private readonly LoaderService _loader;

        public ExtractorService(
            IArtifactRepository artifactRepository,
            ZipManager zipManager,
            IFileNameParser fileNameParser,
            CsvExtractorService csv,
            RowTransformerService transform,
            LoaderService loader)
        {
            _artifactRepository = artifactRepository;
            _zipManager = zipManager;
            _fileNameParser = fileNameParser;
            _csv = csv;
            _transform = transform;
            _loader = loader;
        }

        public async Task<Result> ExtractZipAsync(JobInfo job, RetailerEtlOptions options, JobArtifact zip, Func<JobArtifact, InMemoryFile, CancellationToken, Task> onFileExtractedAsync, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(zip.Identity))
            {
                return new ResultError($"Zip file not found: {zip}");
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<Task<JobArtifact>> tasks = [];
            SemaphoreSlim inflight = new SemaphoreSlim(options.MaxConcurrentItems);

            using (FileStream fs = new(zip.Identity, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive zipArchive = new(fs, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding: Encoding.UTF8))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    JobArtifact zipEntry = new JobArtifact(entry.GetEntryIdentity(zip.Identity), ArtifactType.VirtualFile);
                    if (string.IsNullOrEmpty(entry.Name) || !entry.Name.Is<CsvFileSignature>())
                    {
                        await _artifactRepository.EnsureSkippedAsync(job.Id, entry.GetEntryIdentity(zip.Identity), entry.Length, cancellationToken);
                        job.ReportProgress(new JobProgress { Item = zipEntry, Message = "Entry not a csv. Skipping." });
                        continue;
                    }

                    // Bound how many entries we keep in memory / process concurrently
                    await inflight.WaitAsync(cancellationToken);

                    // Read the entry ONCE while the archive is open
                    InMemoryFile inMemoryFile = await _zipManager.ReadZipEntryInMemoryAsync(zipEntry, entry, cancellationToken);

                    await onFileExtractedAsync(zip, inMemoryFile, cancellationToken);
                    // Schedule independent processing
                    Task<JobArtifact> t = ProcessEntryAsync(job, options, inMemoryFile, inflight, cancellationToken);

                    tasks.Add(t);
                }
            }
            IEnumerable<JobArtifact> processed = await Task.WhenAll(tasks);

            Dictionary<string, ArtifactStatus> statuses = await _artifactRepository.GetStatusByHashesAsync(processed.Select(x => x.Hash), cancellationToken);
            foreach (JobArtifact item in processed)
            {
                if (statuses.TryGetValue(item.Hash, out ArtifactStatus status) && status != ArtifactStatus.Completed && status != ArtifactStatus.Skipped)
                {
                    return new ResultError($"At least one entry in {zip} is not done yet");
                }
            }
            return true;
        }

        private async Task<JobArtifact> ProcessEntryAsync(JobInfo job, RetailerEtlOptions options, InMemoryFile inMemoryFile, SemaphoreSlim inflight, CancellationToken cancellationToken)
        {
            JobArtifact artifact = new JobArtifact(inMemoryFile);
            try
            {
                List<LineSnapshot> lines = new List<LineSnapshot>();
                using (inMemoryFile)
                {
                    ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(job.Id, artifact, inMemoryFile.Content.Length, cancellationToken);
                    if (status == ArtifactStatus.Completed)
                    {
                        job.ReportProgress(new JobProgress { Item = artifact, Skipped = 1, Message = "Entry already completed. Skipping." });
                        return artifact;
                    }

                    (Store store, DateOnly date) meta = _fileNameParser.Parse(inMemoryFile.FullName);

                    inMemoryFile.Content.Position = 0;
                    Encoding enc = Encoding.GetEncoding(string.IsNullOrWhiteSpace(options.Encoding) ? "windows-1250" : options.Encoding);
                    foreach (CsvRowRaw raw in _csv.ReadRows(inMemoryFile.Content, enc))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return artifact;
                        }

                        LineSnapshot snap = _transform.Transform(options.Retailer, meta.store, meta.date, raw);
                        if (!string.IsNullOrWhiteSpace(snap.Product.CanonicalName) && snap.Price.Price > 0m)
                        {
                            lines.Add(snap);
                        }
                    }
                }

                // 4) Load using only lines + hash (no stream dependency)
                ProgressInfo result = await _loader.LoadDataAsync(lines, cancellationToken);

                await _artifactRepository.MarkCompletedAsync(artifact, result, cancellationToken);

                job.ReportProgress(new JobProgress { Item = artifact, Message = result.ToString() });
            }
            catch (OperationCanceledException)
            {
                await _artifactRepository.MarkFailedAsync(artifact, "Operation cancelled", cancellationToken);
            }
            catch (Exception ex)
            {
                await _artifactRepository.MarkFailedAsync(artifact, $"Exception: {ex}", cancellationToken);
                job.ReportProgress(new JobProgress { Item = artifact, Message = $"Error: {ex}" });
            }
            finally
            {
                inflight.Release();
            }
            return artifact;
        }
    }
}
