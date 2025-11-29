using Microsoft.EntityFrameworkCore;
using Niles.Chores;
using Niles.Chores.Abstractions.Models;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Contexts;
using Niles.Chores.Mappers;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class HouseKeepingService : IHouseKeepingService
    {
        private readonly ChoresContext _context;
        private readonly ICriticalChoreService _criticalChoreService;

        public HouseKeepingService(ChoresContext context, ICriticalChoreService criticalChoreService)
        {
            _context = context;
            _criticalChoreService = criticalChoreService;
        }

        public async Task<bool> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null) return false;

            var housekeepingDb = new HousekeepingDb
            {
                Timestamp = housekeeping.DateTime,
                Duration = housekeeping.Duration,
                Note = housekeeping.Note,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Housekeeping.Add(housekeepingDb);
            await _context.SaveChangesAsync(cancellationToken);

            // Add completed chores
            if (housekeeping.CompletedChoreIds != null && housekeeping.CompletedChoreIds.Count > 0)
            {
                var completedChores = new List<CompletedChoreDb>();
                foreach (var choreId in housekeeping.CompletedChoreIds)
                {
                    var completedChore = new CompletedChoreDb
                    {
                        ChoreId = choreId,
                        HousekeepingId = housekeepingDb.Id
                    };
                    _context.CompletedChores.Add(completedChore);
                    completedChores.Add(completedChore);
                }
                await _context.SaveChangesAsync(cancellationToken);

                // Mark critical chores as completed if they exist
                var choreIdToCompletedChoreIdMap = completedChores
                    .ToDictionary(cc => cc.ChoreId, cc => cc.Id);
                await _criticalChoreService.MarkCriticalChoresAsCompletedAsync(choreIdToCompletedChoreIdMap, cancellationToken);
            }

            housekeeping.Id = housekeepingDb.Id;
            return true;
        }

        public async Task<Housekeeping?> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            var housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (housekeepingDb == null) return null;

            return housekeepingDb.ToHousekeeping();
        }

        public async Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default)
        {
            var housekeepingsDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .ToListAsync(cancellationToken);

            return housekeepingsDb.Select(h => h.ToHousekeeping());
        }

        public async Task<PagedResult<Housekeeping>> ListHousekeepingsPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            // Build query with search filter
            var query = _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLowerInvariant();
                
                // Try to parse as date
                DateOnly? searchDate = null;
                if (DateOnly.TryParse(search, out var parsedDate))
                {
                    searchDate = parsedDate;
                }
                
                // Try to parse as integer (for chore ID search)
                int? searchChoreId = null;
                if (int.TryParse(search, out var parsedChoreId))
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
            var total = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering at database level
            var housekeepingsDb = await query
                .OrderByDescending(h => h.Timestamp) // Most recent first
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = housekeepingsDb.Select(h => h.ToHousekeeping());
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            return new PagedResult<Housekeeping>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            };
        }

        public async Task<bool> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null || !housekeeping.Id.HasValue) return false;

            var housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == housekeeping.Id.Value, cancellationToken);

            if (housekeepingDb == null) return false;

            housekeepingDb.Timestamp = housekeeping.DateTime;
            housekeepingDb.Duration = housekeeping.Duration;
            housekeepingDb.Note = housekeeping.Note;
            housekeepingDb.UpdatedAt = DateTimeOffset.UtcNow;

            // Update completed chores
            var existingChoreIds = housekeepingDb.CompletedChores.Select(c => c.ChoreId).ToList();
            var newChoreIds = housekeeping.CompletedChoreIds ?? new List<int>();

            // Remove completed chores that are no longer in the list
            var toRemove = housekeepingDb.CompletedChores
                .Where(c => !newChoreIds.Contains(c.ChoreId))
                .ToList();
            foreach (var completedChore in toRemove)
            {
                _context.CompletedChores.Remove(completedChore);
            }

            // Add new completed chores
            var toAdd = newChoreIds
                .Where(id => !existingChoreIds.Contains(id))
                .Select(choreId => new CompletedChoreDb
                {
                    ChoreId = choreId,
                    HousekeepingId = housekeepingDb.Id
                });
            await _context.CompletedChores.AddRangeAsync(toAdd, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            var housekeepingDb = await _context.Housekeeping
                .Include(h => h.CompletedChores)
                    .ThenInclude(cc => cc.Chore)
                .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            if (housekeepingDb == null) return false;

            // Remove completed chores first
            _context.CompletedChores.RemoveRange(housekeepingDb.CompletedChores);
            _context.Housekeeping.Remove(housekeepingDb);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
