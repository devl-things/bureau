using Bureau.Core;
using Niles.Etl.Configurations;
using Niles.Etl.Jobs;

namespace Niles.Etl.Extract
{
    public interface IDownloader
    {
        public Task<Result> DownloadAsync(JobInfo job, RetailerEtlOptions options, Func<JobArtifact, CancellationToken, Task> onDownloadReadyAsync, CancellationToken cancellationToken = default);
    }
}
