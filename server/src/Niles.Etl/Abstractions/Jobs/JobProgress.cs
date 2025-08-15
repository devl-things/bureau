namespace Niles.Etl.Jobs
{
    public sealed class JobProgress
    {
        public int? TotalFound { get; init; }
        public int? Processed { get; init; }
        public int? Skipped { get; init; }
        public string? CurrentItem { get; init; }
        public string? Message { get; init; }
        public string? Error { get; init; }
    }
}
