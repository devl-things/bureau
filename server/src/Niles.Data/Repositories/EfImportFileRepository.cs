using Microsoft.EntityFrameworkCore;
using Niles.Data.Contexts;
using Niles.Data.Models;

namespace Niles.Data.Repositories
{
    public sealed class EfImportFileRepository : IArtifactRepository
    {
        private readonly NilesContext _context;
        public EfImportFileRepository(NilesContext db)
        {
            _context = db;
        }

        /// <summary>
        /// Looks up statuses for a batch of SHA-256 hashes and returns a dictionary of found entries.
        /// Missing hashes are omitted from the result (similar to the single-hash API returning null).
        /// </summary>
        public async Task<Dictionary<string, ArtifactStatus>> GetStatusByHashesAsync(IReadOnlyList<string> hashes, CancellationToken ct)
        {
            Dictionary<string, ArtifactStatus> result = new Dictionary<string, ArtifactStatus>(hashes.Count, StringComparer.OrdinalIgnoreCase);

            if (hashes.Count == 0)
            {
                return result;
            }

            List<string> distinct = [.. hashes.Where(h => !string.IsNullOrWhiteSpace(h)).Distinct(StringComparer.OrdinalIgnoreCase)];

            const int chunkSize = 1000;
            for (int i = 0; i < distinct.Count; i += chunkSize)
            {
                HashSet<string> slice = [.. distinct.Skip(i).Take(chunkSize)];

                // Select only the columns we need.
                List<KeyValuePair<string, ImportFileStatusDb>> rows =
                    await _context.ImportFiles
                        .AsNoTracking()
                        .Where(x => slice.Contains(x.Sha256))
                        .Select(x => new KeyValuePair<string, ImportFileStatusDb>(x.Sha256, x.Status))
                        .ToListAsync(ct);

                for (int r = 0; r < rows.Count; r++)
                {
                    string hash = rows[r].Key;
                    ArtifactStatus? mapped = ToStatus(rows[r].Value);
                    if (mapped.HasValue && !result.ContainsKey(hash))
                    {
                        result.Add(hash, mapped.Value);
                    }
                }
            }

            return result;
        }

        private static ArtifactStatus? ToStatus(ImportFileStatusDb status)
        {
            switch (status)
            {
                case ImportFileStatusDb.Pending: return ArtifactStatus.Pending;
                case ImportFileStatusDb.InProgress: return ArtifactStatus.InProgress;
                case ImportFileStatusDb.Completed: return ArtifactStatus.Completed;
                case ImportFileStatusDb.Skipped: return ArtifactStatus.Skipped;
                case ImportFileStatusDb.Failed: return ArtifactStatus.Failed;
                default: return null;
            }
        }


        public async Task<ArtifactStatus?> GetStatusByHashAsync(string sha256, CancellationToken ct)
        {
            ImportFileDb? r = await _context.ImportFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Sha256 == sha256, ct);
            if (r == null) return null;
            return ToStatus(r.Status);
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
            _context.ImportFiles.Add(row);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkInProgressAsync(string sha256, CancellationToken ct)
        {
            ImportFileDb row = await _context.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.InProgress;
            row.StartedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkCompletedAsync(string sha256, int rowsFound, int rowsInserted, int rowsUpserted, CancellationToken ct)
        {
            ImportFileDb row = await _context.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.Completed;
            row.RowsFound = rowsFound;
            row.RowsInserted = rowsInserted;
            row.RowsUpserted = rowsUpserted;
            row.FinishedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkFailedAsync(string sha256, string error, CancellationToken ct)
        {
            ImportFileDb row = await _context.ImportFiles.FirstAsync(x => x.Sha256 == sha256, ct);
            row.Status = ImportFileStatusDb.Failed;
            row.Error = error;
            row.FinishedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}
