using Bureau;
using Microsoft.EntityFrameworkCore;
using Niles.Chores.Contexts;
using Niles.Chores.Mappers;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class ChoreService : IChoreService
    {
        private readonly ChoresContext _context;
        private readonly ICriticalChoreService _criticalChoreService;
        private readonly TimeProvider _timeProvider;

        public ChoreService(ChoresContext context, ICriticalChoreService criticalChoreService, TimeProvider timeProvider)
        {
            _context = context;
            _criticalChoreService = criticalChoreService;
            _timeProvider = timeProvider;
        }

        public async Task<Result<Chore>> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null)
            {
                return "Chore cannot be null";
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            ChoreDb choreDb = new ChoreDb
            {
                Title = chore.Title,
                Description = chore.Description,
                Type = chore.Type,
                WeeklyInterval = chore.WeeklyInterval,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Chores.Add(choreDb);
            await _context.SaveChangesAsync(cancellationToken);

            chore.Id = choreDb.Id;
            return chore;
        }

        public async Task<Result<Chore>> GetChoreAsync(int id, CancellationToken cancellationToken)
        {
            ChoreDetail? chore = await _context.Chores.Where(c => c.Id == id)
                .Select(chore => new ChoreDetail
                {
                    Chore = chore,
                    OpenCritical = chore.CriticalChores
                        .Where(c => c.CompletedChoreId == null)
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new CriticalChoreDetail
                        {
                            CreatedAt = c.CreatedAt,
                            Note = c.Note
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (chore == null) return "Not found";
            return chore.ToChore();
        }

        public async Task<PagedResult<Chore>> ListChoresPagedAsync(SearchParameters pagination, CancellationToken cancellationToken = default)
        {
            // Build query with search filter
            IQueryable<ChoreDb> query = _context.Chores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                string searchLower = pagination.Search.ToLowerInvariant();

                // Try to parse search term as ChoreType enum
                ChoreType? searchType = null;
                if (Enum.TryParse<ChoreType>(pagination.Search, true, out ChoreType parsedType))
                {
                    searchType = parsedType;
                }

                query = query.Where(c =>
                    (c.Title != null && c.Title.ToLower().Contains(searchLower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchLower)) ||
                    (searchType.HasValue && c.Type == searchType.Value)
                );
            }

            // Get total count (before pagination)
            int total = await query.CountAsync(cancellationToken);

            // Apply pagination at database level
            List<Chore> items = await query
                .OrderBy(c => c.Id) // Consistent ordering
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(x => x.ToChore())
                .ToListAsync(cancellationToken);

            return new PagedResult<Chore>(items, pagination, total);
        }

        public async Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null || chore.Id == 0)
            {
                return "Chore cannot be null and must have a valid Id";
            }

            ChoreDb? choreDb = await _context.Chores.FindAsync(new object[] { chore.Id }, cancellationToken);
            if (choreDb == null)
            {
                return $"Chore with Id {chore.Id} not found";
            }

            choreDb.Title = chore.Title;
            choreDb.Description = chore.Description;
            choreDb.Type = chore.Type;
            choreDb.WeeklyInterval = chore.WeeklyInterval;
            choreDb.UpdatedAt = _timeProvider.GetUtcNow();

            await _context.SaveChangesAsync(cancellationToken);

            Chore updatedChore = choreDb.ToChore();
            return updatedChore;
        }

        public async Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            int rowsAffected = await _context.Chores
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            if (rowsAffected == 0)
            {
                return $"Chore with Id {id} not found";
            }

            return true;

        }

        //TODO REFACTOR This method is not filling the IsCritical property on the Chore objects
        //TODO REFACTOR Consider renaming to GetChoresAsync and have only this method and delete ListChoresPagedAsync or move here the logic
        public async Task<PagedResult<Chore>> ListChoresPagedWithCriticalAsync(SearchParameters pagination, CancellationToken cancellationToken = default)
        {
            PagedResult<Chore> pagedResult = await ListChoresPagedAsync(pagination, cancellationToken);

            // Get all chore IDs for this page
            List<int> choreIds = pagedResult.Values.Select(c => c.Id).ToList();

            // Get all open critical chores for these chore IDs in one query
            List<int> openCriticalChoreIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(choreIds, cancellationToken);

            return pagedResult;
        }
    }
}
