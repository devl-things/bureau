namespace Niles.Etl.Jobs
{
    public sealed class JobStatus
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public JobType Type { get; init; }
        public JobState State { get; set; } = JobState.Queued;

        public int TotalFound { get; set; }
        public int Processed { get; set; }
        public int Skipped { get; set; }
        public string? CurrentItem { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; } = new List<string>();

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
    }
}
