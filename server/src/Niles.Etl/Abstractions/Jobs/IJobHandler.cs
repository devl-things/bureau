namespace Niles.Etl.Jobs
{
    public interface IJobHandler
    {
        JobType Type { get; }
        Task HandleAsync(string jobId, Dictionary<string, string>? args, IJobReporter reporter, CancellationToken cancellationToken = default);
    }
}
