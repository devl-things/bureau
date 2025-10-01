using Bureau.Core;

namespace Niles.Etl.Jobs
{
    public sealed class JobProgress : ProgressInfo
    {
        public JobArtifact? Item { get; init; }
        public string? Message { get; init; }
        public string? Error { get; init; }
        public int? Processed { get; init; }
        public int? Skipped { get; init; }

    }
}
