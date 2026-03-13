namespace Niles.Etl.Jobs
{
    public record struct JobInfo
    {
        private readonly IJobReporter _reporter;
        public string Id { get; set; }
        public JobType Type { get; set; }
        public JobInfo(string id, JobType type, IJobReporter reporter)
        {
            Id = id;
            Type = type;
            _reporter = reporter;
        }
        public void ReportProgress(JobProgress progress)
        {
            _reporter.Report(Id, progress);
        }
    }

    public class JobWorkItemBase
    {
        public required string JobId { get; init; }
        public required Dictionary<string, string>? Args { get; init; }
    }
    public sealed class JobWorkItem : JobWorkItemBase
    {
        public required JobType Type { get; init; }
        public required CancellationTokenSource Cts { get; init; }
    }
}
