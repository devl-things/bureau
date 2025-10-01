namespace Niles.Data.Models
{
    public sealed class ImportFileDb
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTimeOffset DiscoveredAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public ImportFileStatusDb Status { get; set; }
        public string JobId { get; set; } = string.Empty;
        public int RowsFound { get; set; }
        public int RowsInserted { get; set; }
        public int RowsUpserted { get; set; }
        public string? Error { get; set; }
    }

    public enum ImportFileStatusDb { Pending, InProgress, Completed, Skipped, Failed }
}
