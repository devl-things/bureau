using Microsoft.EntityFrameworkCore;
using Niles.Data.Contexts;
using Niles.Data.Models;

namespace Niles.Data.Repositories
{
    public sealed class EfImportFileRepository : IImportFileRepository
    {
        private readonly NilesContext _db;
        public EfImportFileRepository(NilesContext db) { _db = db; }

        public async Task<ImportFileStatus?> GetStatusByHashAsync(string sha256, CancellationToken ct)
        {
            ImportFileDb? r = await _db.ImportFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Sha256 == sha256, ct);
            if (r == null) return null;
            return r.Status switch
            {
                ImportFileStatusDb.Pending => ImportFileStatus.Pending,
                ImportFileStatusDb.InProgress => ImportFileStatus.InProgress,
                ImportFileStatusDb.Completed => ImportFileStatus.Completed,
                ImportFileStatusDb.Failed => ImportFileStatus.Failed,
                _ => null
            };
        }

        public async Task CreatePendingAsync(string jobId, string fileName, string fullPath, string sha256, long sizeBytes, CancellationToken ct)
        {
            ImportFileDb row = new ImportFileDb
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                FileName = fileName,
                FullPath = fullPath,
                Sha256 = sha256,
                SizeBytes = sizeBytes,
                DiscoveredAt = DateTimeOffset.UtcNow,
                Status = ImportFileStatusDb.Pending
            };
            _db.ImportFiles.Add(row);
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkInProgressAsync(string sha256, CancellationToken ct)
        {
            ImportFileDb row = await _db.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.InProgress;
            row.StartedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkCompletedAsync(string sha256, int rowsFound, int rowsInserted, int rowsUpserted, CancellationToken ct)
        {
            ImportFileDb row = await _db.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.Completed;
            row.RowsFound = rowsFound;
            row.RowsInserted = rowsInserted;
            row.RowsUpserted = rowsUpserted;
            row.FinishedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkFailedAsync(string sha256, string error, CancellationToken ct)
        {
            ImportFileDb row = await _db.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.Failed;
            row.Error = error;
            row.FinishedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
