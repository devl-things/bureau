namespace Niles.Etl.Jobs
{
    public interface IJobDispatcher
    {
        Task DispatchAsync(JobWorkItem work, IJobReporter reporter, CancellationToken cancellationToken = default);
    }
}
