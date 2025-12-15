namespace Niles.Etl.Jobs
{
    public interface IJobManager : IJobReporter
    {
        Task<string> EnqueueAsync(JobType type, Dictionary<string, string>? args, CancellationToken ct = default);
        bool TryGetStatus(string id, out JobStatus status);
        IReadOnlyCollection<JobStatus> GetAll(JobType? type = null, JobState? state = null);
        bool Cancel(string id);
        IAsyncEnumerable<JobWorkItem> ReadAllAsync(CancellationToken stoppingToken);
        void MarkRunning(string jobId);
        void MarkCompleted(string jobId);
        void MarkFailed(string jobId, string error);
        void MarkCanceled(string jobId);
    }
}
