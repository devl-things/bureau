using Bureau.Core;

namespace Niles.Etl.Jobs
{
    public interface IJobDispatcher
    {
        Task<Result> DispatchAsync(JobWorkItem work, IJobReporter reporter, CancellationToken cancellationToken = default);
    }
}
