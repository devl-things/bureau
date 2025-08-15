namespace Niles.Data
{
    public enum ImportFileStatus { Pending, InProgress, Completed, Failed }
    public interface IImportFileRepository
    {
        Task<ImportFileStatus?> GetStatusByHashAsync(string sha256, CancellationToken ct);
        Task CreatePendingAsync(string jobId, string fileName, string fullPath, string sha256, long sizeBytes, CancellationToken ct);
        Task MarkInProgressAsync(string sha256, CancellationToken ct);
        Task MarkCompletedAsync(string sha256, int rowsFound, int rowsInserted, int rowsUpserted, CancellationToken ct);
        Task MarkFailedAsync(string sha256, string error, CancellationToken ct);
    }
}
