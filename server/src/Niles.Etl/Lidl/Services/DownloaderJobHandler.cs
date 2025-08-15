using Niles.Etl.Jobs;
using Niles.Etl.Lidl.Models;

namespace Niles.Etl.Lidl.Services
{
    public sealed class DownloaderJobHandler : IJobHandler
    {
        private readonly DownloaderService _downloader;

        public DownloaderJobHandler(DownloaderService downloader)
        {
            _downloader = downloader;
        }

        public JobType Type => JobType.Downloader;

        public Task HandleAsync(string jobId, Dictionary<string, string>? args, IJobReporter reporter, CancellationToken cancellationToken = default)
        {
            DownloaderJobArgs parsed = DownloaderJobArgs.From(args);
            return _downloader.DownloadAndExtractPricesAsync(reporter, jobId, parsed, cancellationToken);
        }
    }
}
