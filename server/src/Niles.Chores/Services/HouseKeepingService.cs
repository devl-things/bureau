using Bureau;
using Microsoft.EntityFrameworkCore;
using Niles.Chores.Contexts;
using Niles.Chores.Mappers;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class HouseKeepingService : IHouseKeepingService
    {
        private readonly ChoresContext _context;
        private readonly ICriticalChoreService _criticalChoreService;
        private readonly TimeProvider _timeProvider;

        public HouseKeepingService(ChoresContext context, ICriticalChoreService criticalChoreService, TimeProvider timeProvider)
        {
            _context = context;
            _criticalChoreService = criticalChoreService;
            _timeProvider = timeProvider;
        }

        public async Task<Result<Housekeeping>> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null)
            {
                return "Housekeeping cannot be null";
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            HousekeepingDb housekeepingDb = new HousekeepingDb
            {
                Timestamp = housekeeping.DateTime,
                Duration = housekeeping.Duration,
                Note = housekeeping.Note,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Housekeeping.Add(housekeepingDb);
            await _context.SaveChangesAsync(cancellationToken);

            // Add completed chores
            if (housekeeping.CompletedChoreIds != null && housekeeping.CompletedChoreIds.Count > 0)
            {
                List<CompletedChoreDb> completedChores = new List<CompletedChoreDb>();
                foreach (int choreId in housekeeping.CompletedChoreIds)
                {
                    CompletedChoreDb completedChore = new CompletedChoreDb
                    {
                        ChoreId = choreId,
                        HousekeepingId = housekeepingDb.Id
                    };
                    _context.CompletedChores.Add(completedChore);
                    completedChores.Add(completedChore);
                }
                await _context.SaveChangesAsync(cancellationToken);

                // Mark critical chores as completed if they exist
                Dictionary<int, int> choreIdToCompletedChoreIdMap = completedChores
                    .ToDictionary(cc => cc.ChoreId, cc => cc.Id);
                await _criticalChoreService.MarkCriticalChoresAsCompletedAsync(choreIdToCompletedChoreIdMap, cancellationToken);
            }

            housekeeping.Id = housekeepingDb.Id;

            // Reload housekeeping with completed chores and their chore navigation properties
            HousekeepingDb? housekeepingDbWithChores = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == housekeepingDb.Id, cancellationToken);

            Housekeeping createdHousekeeping = housekeepingDbWithChores!.ToHousekeeping();
            return createdHousekeeping;
        }

        public async Task<Result<Housekeeping>> GetHousekeepingAsync(int id, CancellationToken cancellationToken)
        {
            HousekeepingDb? housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (housekeepingDb == null) return "Not found";

            return housekeepingDb.ToHousekeeping();
        }

        public async Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default)
        {
            List<HousekeepingDb> housekeepingsDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .ToListAsync(cancellationToken);

            return housekeepingsDb.Select(h => h.ToHousekeeping());
        }

        public async Task<PagedResult<Housekeeping>> ListHousekeepingsPagedAsync(SearchParameters pagination, CancellationToken cancellationToken = default)
        {
            // Build query with search filter
            IQueryable<HousekeepingDb> query = _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                string searchLower = pagination.Search.ToLowerInvariant();

                // Try to parse as date
                DateOnly? searchDate = null;
                if (DateOnly.TryParse(pagination.Search, out DateOnly parsedDate))
                {
                    searchDate = parsedDate;
                }

                // Try to parse as integer (for chore ID search)
                int? searchChoreId = null;
                if (int.TryParse(pagination.Search, out int parsedChoreId))
                {
                    searchChoreId = parsedChoreId;
                }

                query = query.Where(h =>
                    (h.Note != null && h.Note.ToLower().Contains(searchLower)) ||
                    (searchDate.HasValue && DateOnly.FromDateTime(h.Timestamp.Date) == searchDate.Value) ||
                    (searchChoreId.HasValue && h.CompletedChores.Any(cc => cc.ChoreId == searchChoreId.Value))
                );
            }

            // Get total count (before pagination)
            int total = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering at database level
            List<Housekeeping> housekeepings = await query
                .OrderByDescending(h => h.Timestamp) // Most recent first
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(x => x.ToHousekeeping())
                .ToListAsync(cancellationToken);

            return new PagedResult<Housekeeping>(housekeepings, pagination, total);
        }

        public async Task<Result<Housekeeping>> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null || !housekeeping.Id.HasValue)
            {
                return "Housekeeping cannot be null and must have a valid Id";
            }

            HousekeepingDb? housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == housekeeping.Id.Value, cancellationToken);

            if (housekeepingDb == null)
            {
                return $"Housekeeping with Id {housekeeping.Id.Value} not found";
            }

            housekeepingDb.Timestamp = housekeeping.DateTime;
            housekeepingDb.Duration = housekeeping.Duration;
            housekeepingDb.Note = housekeeping.Note;
            housekeepingDb.UpdatedAt = _timeProvider.GetUtcNow();

            // Update completed chores
            List<int> existingChoreIds = housekeepingDb.CompletedChores.Select(c => c.ChoreId).ToList();
            List<int> newChoreIds = housekeeping.CompletedChoreIds ?? new List<int>();

            // Remove completed chores that are no longer in the list
            List<CompletedChoreDb> toRemove = housekeepingDb.CompletedChores
                .Where(c => !newChoreIds.Contains(c.ChoreId))
                .ToList();
            foreach (CompletedChoreDb completedChore in toRemove)
            {
                _context.CompletedChores.Remove(completedChore);
            }

            // Add new completed chores
            IEnumerable<CompletedChoreDb> toAdd = newChoreIds
                .Where(id => !existingChoreIds.Contains(id))
                .Select(choreId => new CompletedChoreDb
                {
                    ChoreId = choreId,
                    HousekeepingId = housekeepingDb.Id
                });
            await _context.CompletedChores.AddRangeAsync(toAdd, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            Housekeeping updatedHousekeeping = housekeepingDb.ToHousekeeping();
            return updatedHousekeeping;
        }

        public async Task<Result> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            HousekeepingDb? housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (housekeepingDb == null)
            {
                return $"Housekeeping with Id {id} not found";
            }

            // Remove completed chores first
            _context.CompletedChores.RemoveRange(housekeepingDb.CompletedChores);
            _context.Housekeeping.Remove(housekeepingDb);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
