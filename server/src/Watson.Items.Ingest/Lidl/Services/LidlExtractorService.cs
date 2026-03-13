using Bureau.Core;
using Bureau.Core.FileSignature;
using Niles.Data;
using Niles.Etl.Configurations;
using Niles.Etl.Jobs;
using Niles.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Text;

namespace Niles.Etl.Lidl.Services
{
    internal class LidlExtractorService
    {
        private readonly IArtifactRepository _artifactRepository;
        private readonly ZipManager _zipManager;

        private sealed record RunContext(
            JobInfo Job,
            RetailerEtlOptions Options,
            JobArtifact Zip,
            Func<JobArtifact, InMemoryFile, CancellationToken, Task> OnExtractionFinishedAsync
        );

        public LidlExtractorService(IArtifactRepository artifactRepository, ZipManager zipManager)
        {
            _artifactRepository = artifactRepository;
            _zipManager = zipManager;
        }

        public async Task<Result> ExtractAsync(JobInfo job, RetailerEtlOptions options, JobArtifact zip, Func<JobArtifact, InMemoryFile, CancellationToken, Task> onExtractionFinishedAsync, CancellationToken cancellationToken = default)
        {
            Result<RunContext> validationResult = ValidateAndCreateContext(job, options, zip, onExtractionFinishedAsync);
            if (validationResult.IsError) return validationResult.Error;

            RunContext context = validationResult.Value;
            try
            {
                ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(context.Job.Id, context.Zip, cancellationToken);
                if (!context.Options.Force && status.IsProcessed())
                {
                    context.Job.ReportProgress(new JobProgress { Item = context.Zip, Skipped = 1, Message = "Entry already completed. Skipping." });
                    return false;
                }

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                job.ReportProgress(new JobProgress { Item = context.Zip, Message = $"Exporter (ZIP only). {context.Zip} " });

                List<Task<ArtifactStatus>> tasks = new();

                int found = 0;
                using (FileStream fs = new(zip.Identity, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (ZipArchive zipArchive = new(fs, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding: Encoding.UTF8))
                {
                    found = zipArchive.Entries.Count;
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        if (cancellationToken.IsCancellationRequested) return false;

                        tasks.Add(HandleExtractionAsync(context, entry, cancellationToken));
                    }
                }

                ArtifactStatus[] results = await Task.WhenAll(tasks);

                int extracted = 0, skipped = 0;
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i] == ArtifactStatus.Skipped)
                    {
                        skipped++;
                    }
                    else if (results[i] == ArtifactStatus.Completed)
                    {
                        extracted++;
                    }
                }
                await _artifactRepository.MarkCompletedAsync(context.Zip, cancellationToken);
                job.ReportProgress(new JobProgress { Item = zip, Found = found, Message = $"Extraction phase finished. Extracted={extracted}, Skipped={skipped}." });
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                await _artifactRepository.MarkFailedAsync(context.Zip, ex.ToString(), cancellationToken);
                return new ResultError(ex);
            }
        }

        private Result<RunContext> ValidateAndCreateContext(JobInfo job, RetailerEtlOptions options, JobArtifact zip, Func<JobArtifact, InMemoryFile, CancellationToken, Task> onExtractionFinishedAsync)
        {
            if (onExtractionFinishedAsync == null) return new ResultError($"{nameof(onExtractionFinishedAsync)} not defined");
            if (zip == null) return new ResultError($"{nameof(zip)} not defined");
            if (!File.Exists(zip.Identity)) return new ResultError($"Zip file not found: {zip}");
            RetailerEtlOptions opt = options ?? new RetailerEtlOptions();

            return new RunContext(job, opt, zip, onExtractionFinishedAsync);
        }

        private async Task<ArtifactStatus> HandleExtractionAsync(RunContext context, ZipArchiveEntry entry, CancellationToken cancellationToken = default)
        {
            JobArtifact zipEntry = new JobArtifact(entry.GetEntryIdentity(context.Zip.Identity), ArtifactType.VirtualFile);

            ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(context.Job.Id, zipEntry, cancellationToken);
            if (!context.Options.Force && status.IsProcessed())
            {
                context.Job.ReportProgress(new JobProgress { Item = zipEntry, Skipped = 1, Message = "Entry already completed. Skipping." });
                return ArtifactStatus.Skipped;
            }

            if (string.IsNullOrEmpty(entry.Name) || !entry.Name.Is<CsvFileSignature>())
            {
                await _artifactRepository.MarkSkippedAsync(zipEntry, cancellationToken);
                context.Job.ReportProgress(new JobProgress { Item = zipEntry, Message = "Entry not a csv. Skipping." });
                return ArtifactStatus.Skipped;
            }

            // Read the entry ONCE while the archive is open
            InMemoryFile inMemoryFile = await _zipManager.ReadZipEntryInMemoryAsync(zipEntry, entry, cancellationToken);

            await _artifactRepository.MarkCompletedAsync(zipEntry, cancellationToken);
            await context.OnExtractionFinishedAsync(zipEntry, inMemoryFile, cancellationToken);
            return ArtifactStatus.Completed;
        }



        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property, bool descending = false)
        {
            Expression<Func<T, string>> expression = x => x.ToString() ?? string.Empty;
            Expression<Func<T, int>> expression2 = x => 1;
            List<Expression<Func<T, object>>> selectors = new List<Expression<Func<T, object>>>();

            selectors.Add(expression2);
            

            if (string.IsNullOrWhiteSpace(property)) throw new ArgumentException("Property is required.", nameof(property));

            ParameterExpression p = Expression.Parameter(typeof(T), "x");
            MethodCallExpression body = BuildEfPropertyChain(p, property);


            var keySelector = Expression.Lambda<Func<T, object>>(body, p);
            LambdaExpression converted = Expression.Lambda<Func<T, object>>(Expression.Convert(body, typeof(object)), p);

            


            IQueryable r = Queryable.ThenBy(source, converted);

            return descending ? Queryable.ThenByDescending(source, keySelector)
                              : Queryable.ThenBy(source, keySelector);
        }

        public static IOrderedQueryable<T> OrderBy<T>(
        this IQueryable<T> source, string property, bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(property)) throw new ArgumentException("Property is required.", nameof(property));

            // x => EF.Property<object>(x, property)
            ParameterExpression p = Expression.Parameter(typeof(T), "x");
            MethodCallExpression body = BuildEfPropertyChain(p, property);

            var keySelector = Expression.Lambda<Func<T, object>>(body, p);

            LambdaExpression converted = Expression.Lambda<Func<T, object>>(Expression.Convert(body, typeof(object)), p);
            return descending ? Queryable.OrderByDescending(source, converted)
                              : Queryable.OrderBy(source, keySelector);
        }
    }
}
