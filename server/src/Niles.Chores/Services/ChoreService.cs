using Bureau;
using Bureau.Primitives.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Niles.Chores.Contexts;
using Niles.Chores.Extensions;
using Niles.Chores.Mappers;
using Niles.Chores.Models;
using Niles.Chores.Utilities;

namespace Niles.Chores.Services
{
    internal class ChoreService : IChoreService
    {
        private readonly ChoresContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IMemoryCache _cache;
        public ChoreService(ChoresContext context, TimeProvider timeProvider, IMemoryCache cache)
        {
            _context = context;
            _timeProvider = timeProvider;
            _cache = cache;
        }

        public async Task<Result<Chore>> GetChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            ChoreDetail? chore = await _context.Chores
                .Where(c => c.Id == id)
                .SelectChoreWithCritical()
                .FirstOrDefaultAsync(cancellationToken);
            if (chore == null) return ResultError.FromLogMessage(ProblemCodes.Resource.NotFound, string.Format(LogMessages.EntityNotFound, nameof(Chore), id));
            return chore.ToChore();
        }

        public async Task<PagedResult<Chore>> GetChoresAsync(SearchParameters searchParameters, CancellationToken cancellationToken = default)
        {
            IQueryable<ChoreDb> query = _context.Chores.AsQueryable();


            if (!string.IsNullOrWhiteSpace(searchParameters.Search))
            {
                ChoreType? searchType = null;
                if (Enum.TryParse<ChoreType>(searchParameters.Search, true, out ChoreType parsedType))
                {
                    searchType = parsedType;
                }

                query = query.Where(c =>
                    (c.Title != null && c.Title.Contains(searchParameters.Search, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Description != null && c.Description.Contains(searchParameters.Search, StringComparison.OrdinalIgnoreCase)) ||
                    (searchType.HasValue && c.Type == searchType.Value)
                );
            }

            IQueryable<ChoreDetail> finalQuery = query.SelectChoreWithCritical();


            // Get total count (before pagination)
            int total = await finalQuery.CountAsync(cancellationToken);

            // Apply pagination at database level
            List<Chore> items = await finalQuery
                .OrderBy(c => c.Chore.Id) // Consistent ordering
                .Skip(searchParameters.SkipRecords)
                .Take(searchParameters.PageSize)
                .Select(x => x.ToChore())
                .ToListAsync(cancellationToken);

            return new PagedResult<Chore>(items, searchParameters, total);
        }

        public async Task<Result<Chore>> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null)
            {
                return ResultError.From(ProblemCodes.Operation.UnexpectedError, "Chore is null");
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

            _cache.RemovePriotizedChores();
            chore.Id = choreDb.Id;
            return chore;
        }

        public async Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null || chore.Id == 0)
            {
                return ResultError.From(ProblemCodes.Operation.UnexpectedError, "Chore is null or it has invalid Id");
            }

            ChoreDb? choreDb = await _context.Chores.FindAsync(new object[] { chore.Id }, cancellationToken);
            if (choreDb == null)
            {
                return ResultError.FromLogMessage(ProblemCodes.Resource.NotFound, string.Format(LogMessages.EntityNotFound, nameof(Chore), chore.Id));
            }

            choreDb.Title = chore.Title;
            choreDb.Description = chore.Description;
            choreDb.Type = chore.Type;
            choreDb.WeeklyInterval = chore.WeeklyInterval;
            choreDb.UpdatedAt = _timeProvider.GetUtcNow();

            await _context.SaveChangesAsync(cancellationToken);

            _cache.RemovePriotizedChores();
            Chore updatedChore = choreDb.ToChore();
            return updatedChore;
        }

        public async Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            int rowsAffected = await _context.Chores
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return ResultError.FromLogMessage(ProblemCodes.Resource.NotFound, string.Format(LogMessages.EntityNotFound, nameof(Chore), id));
            }
            _cache.RemovePriotizedChores();
            return true;

        }

    }
}
