using Bureau.Core;

namespace Niles.Etl.Jobs
{
    public interface IJobHandler
    {
        JobType Type { get; }
        Task<Result> HandleAsync(JobWorkItemBase workItem, IJobReporter reporter, CancellationToken cancellationToken = default);
    }
}
