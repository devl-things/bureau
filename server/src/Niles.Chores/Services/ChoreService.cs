using Microsoft.EntityFrameworkCore;
using Niles.Chores;
using Niles.Chores.Abstractions.Models;
using Niles.Chores.Abstractions.Services;
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
                return new Result<Chore>
                {
                    IsSuccess = false,
                    ErrorMessage = "Chore cannot be null"
                };
            }

            var now = _timeProvider.GetUtcNow();
            var choreDb = new ChoreDb
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
            return new Result<Chore>
            {
                Value = chore,
                IsSuccess = true
            };
        }

        public async Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var choreDb = await _context.Chores.FindAsync(new object[] { id }, cancellationToken);
            if (choreDb == null) return null;

            return choreDb.ToChore();
        }
        
        public async Task<PagedResult<Chore>> ListChoresPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            // Build query with search filter
            var query = _context.Chores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLowerInvariant();
                
                // Try to parse search term as ChoreType enum
                ChoreType? searchType = null;
                if (Enum.TryParse<ChoreType>(search, true, out var parsedType))
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
            var total = await query.CountAsync(cancellationToken);

            // Apply pagination at database level
            var choresDb = await query
                .OrderBy(c => c.Id) // Consistent ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = choresDb.Select(c => c.ToChore());
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            return new PagedResult<Chore>
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

        public async Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null || chore.Id == 0)
            {
                return new Result<Chore>
                {
                    IsSuccess = false,
                    ErrorMessage = "Chore cannot be null and must have a valid Id"
                };
            }

            var choreDb = await _context.Chores.FindAsync(new object[] { chore.Id }, cancellationToken);
            if (choreDb == null)
            {
                return new Result<Chore>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chore with Id {chore.Id} not found"
                };
            }

            choreDb.Title = chore.Title;
            choreDb.Description = chore.Description;
            choreDb.Type = chore.Type;
            choreDb.WeeklyInterval = chore.WeeklyInterval;
            choreDb.UpdatedAt = _timeProvider.GetUtcNow();

            await _context.SaveChangesAsync(cancellationToken);

            var updatedChore = choreDb.ToChore();
            return new Result<Chore>
            {
                Value = updatedChore,
                IsSuccess = true
            };
        }

        public async Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var choreDb = await _context.Chores.FindAsync(new object[] { id }, cancellationToken);
            if (choreDb == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chore with Id {id} not found"
                };
            }

            _context.Chores.Remove(choreDb);
            await _context.SaveChangesAsync(cancellationToken);
            return new Result
            {
                IsSuccess = true
            };
        }

        public async Task<PagedResult<Chore>> ListChoresPagedWithCriticalAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var pagedResult = await ListChoresPagedAsync(search, page, pageSize, cancellationToken);
            
            // Get all chore IDs for this page
            var choreIds = pagedResult.Items.Select(c => c.Id).ToList();
            
            // Get all open critical chores for these chore IDs in one query
            var openCriticalChoreIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(choreIds, cancellationToken);
            
            return pagedResult;
        }

        public async Task<(Chore Chore, bool IsCritical)?> GetChoreWithCriticalAsync(int id, CancellationToken cancellationToken = default)
        {
            var chore = await GetChoreAsync(id, cancellationToken);
            if (chore == null) return null;

            var isCritical = await _criticalChoreService.HasOpenCriticalChoreAsync(id, cancellationToken);
            return (chore, isCritical);
        }
    }
}
