namespace Niles.Etl.Jobs
{
    public sealed class JobWorkItem
    {
        public required string JobId { get; init; }
        public required JobType Type { get; init; }
        public required Dictionary<string, string>? Args { get; init; }
        public required CancellationTokenSource Cts { get; init; }
    }
}
