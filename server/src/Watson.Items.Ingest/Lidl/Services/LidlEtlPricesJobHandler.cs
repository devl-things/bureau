using Bureau.Core;
using Microsoft.Extensions.Options;
using Niles.Etl.Configurations;
using Niles.Etl.Jobs;
using Niles.Etl.Transform;
using Niles.IO;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Niles.Etl.Lidl.Services
{
    public sealed class LidlEtlPricesJobHandler : IJobHandler
    {
        private readonly IOptionsMonitor<RetailerEtlOptions> _optionsMonitor;
        IServiceProvider _serviceProvider;

        private readonly LidlDownloaderService _downloader;
        private readonly LidlExtractorService _extractor;
        private readonly IFileManager _fileManager;

        private Channel<JobArtifact> _downloadedZips;
        private Channel<InMemoryFile> _extractedFiles;

        private readonly ConcurrentDictionary<JobArtifact, List<string>> _filesInZip;

        private readonly List<Task> _zipExtractors;
        private readonly List<Task> _fileProcessors;

        private JobInfo _job;
        private RetailerEtlOptions _options;

        public JobType Type { get { return JobType.LidlEtlPrices; } } // add enum value

        public LidlEtlPricesJobHandler(
            IOptionsMonitor<RetailerEtlOptions> optionsMonitor,
            IServiceProvider serviceProvider,
            LidlDownloaderService downloader,
            LidlExtractorService extractor,
            IFileManager fileManager)
        {
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;

            _downloader = downloader;
            _extractor = extractor;
            _fileManager = fileManager;
            _filesInZip = new ConcurrentDictionary<JobArtifact, List<string>>();

            _fileProcessors = new List<Task>();
        }

        public async Task<Result> HandleAsync(JobWorkItemBase workItem, IJobReporter reporter, CancellationToken cancellationToken = default)
        {
            _options = _optionsMonitor.Get(Type.GetRetailerKey()).BindArgs(workItem.Args);
            _job = new JobInfo(workItem.JobId, Type, reporter);
            _downloadedZips = Channel.CreateBounded<JobArtifact>(new BoundedChannelOptions(_options.MaxConcurrentItems)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });
            _extractedFiles = Channel.CreateBounded<InMemoryFile>(new BoundedChannelOptions(_options.MaxConcurrentItems)
            {
                SingleWriter = false,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });
            try
            {
                List<Task> zipExtractors = new List<Task>();
                Task zipDownloader = _downloader.DownloadAsync(_job, _options, OnZipDownloadedAsync, cancellationToken);

                for (int i = 0; i < _options.MaxConcurrentItems; i++)
                {
                    _zipExtractors.Add(ProcessZipAsync(cancellationToken));
                }

                await zipDownloader.ConfigureAwait(false);
            }
            finally
            {
                _downloadedZips.Writer.TryComplete();
            }
            await Task.WhenAll(_zipExtractors).ConfigureAwait(false);

            return true;
        }

        private Task OnZipDownloadedAsync(JobArtifact zip, CancellationToken cancellationToken = default)
        {
            return _downloadedZips.Writer.WriteAsync(zip, cancellationToken).AsTask();
        }

        private Task OnFileExtractedAsync(JobArtifact artifact, InMemoryFile file, CancellationToken cancellationToken = default)
        {
            _filesInZip.AddOrUpdate(artifact, new List<string> { file.Hash }, (key, list) =>
            {
                list.Add(file.Hash);
                return list;
            });
            return _extractedFiles.Writer.WriteAsync(file, cancellationToken).AsTask();
        }

        private async Task ProcessZipAsync(CancellationToken cancellationToken)
        {
            while (await _downloadedZips.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                JobArtifact? zip;
                if (!_downloadedZips.Reader.TryRead(out zip)) continue;

                Task<Result> zipExtractorTask = _extractor.ExtractAsync(_job, _options, zip, OnFileExtractedAsync, cancellationToken);

                for (int i = 0; i < _options.MaxConcurrentItems; i++)
                {
                    _fileProcessors.Add(ProcessFileAsync(cancellationToken));
                }

                Result result = await zipExtractorTask.ConfigureAwait(false);

                if (result.IsError)
                {
                    _job.ReportProgress(new JobProgress() { Item = zip, Error = result.Error.ErrorMessage });
                }

            }
        }

        private async Task ProcessFileAsync(CancellationToken cancellationToken)
        {
            while (await _extractedFiles.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                InMemoryFile? inMemoryFile;
                if (!_extractedFiles.Reader.TryRead(out inMemoryFile)) continue;

                Task<Result> zipExtractorTask = _extractor.ExtractZipAsync(_job, _options, zip, OnFileExtractedAsync, cancellationToken);

                for (int i = 0; i < _options.MaxConcurrentItems; i++)
                {
                    _fileProcessors.Add(ProcessFileAsync(cancellationToken));
                }

                Result result = await zipExtractorTask.ConfigureAwait(false);

                if (result.IsError)
                {
                    _job.ReportProgress(new JobProgress() { Item = zip, Error = result.Error.ErrorMessage });
                }

            }
        }
    }


    //private async Task<Result> CheckAndArchiveAsync() 
    //{
    //    Dictionary<string, ArtifactStatus> statuses = await _artifactRepository.GetStatusByHashesAsync(processed.Select(x => x.Hash), cancellationToken);
    //    foreach (JobArtifact item in processed)
    //    {
    //        if (statuses.TryGetValue(item.Hash, out ArtifactStatus status) && status != ArtifactStatus.Completed && status != ArtifactStatus.Skipped)
    //        {
    //            return new ResultError($"At least one entry in {zip} is not done yet");
    //        }
    //    }

    //    if (result.IsSuccess)
    //    {
    //        result = _fileManager.MoveTo(zip.Identity, _options.ArchiveFolder);
    //        if (result.IsSuccess)
    //        {
    //            _job.ReportProgress(new JobProgress { Item = zip, Message = "ZIP processed successfully." });
    //            continue;
    //        }
    //    }
    //}
}
}
