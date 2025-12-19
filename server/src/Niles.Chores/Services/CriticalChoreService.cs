using Bureau;
using Bureau.Primitives.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Niles.Chores.Contexts;
using Niles.Chores.Models;
using Niles.Chores.Utilities;

namespace Niles.Chores.Services
{
    internal class CriticalChoreService : ICriticalChoreService
    {
        private readonly ChoresContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IMemoryCache _cache;

        public CriticalChoreService(ChoresContext context, TimeProvider timeProvider, IMemoryCache cache)
        {
            _context = context;
            _timeProvider = timeProvider;
            _cache = cache;
        }

        public async Task<Result> CreateCriticalChoreAsync(int choreId, string note, CancellationToken cancellationToken = default)
        {
            // Verify chore exists
            bool choreExists = await _context.Chores.AnyAsync(c => c.Id == choreId, cancellationToken);
            if (!choreExists)
            {
                return ResultError.FromLogMessage(ProblemCodes.Resource.NotFound, string.Format(LogMessages.EntityNotFound, nameof(Chore), choreId));
            }

            // Check if there's already an open critical chore for this chore
            bool existingOpen = await HasOpenCriticalChoreAsync(choreId, cancellationToken);
            if (existingOpen)
            {
                return ResultError.From(ProblemCodes.Resource.Conflict, "This chore already has an open critical status. Please remove it first.");
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            CriticalChoreDb criticalChore = new CriticalChoreDb
            {
                ChoreId = choreId,
                Note = note,
                CompletedChoreId = null, // Not completed yet
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.CriticalChores.Add(criticalChore);
            await _context.SaveChangesAsync(cancellationToken);
            _cache.RemovePriotizedChores();
            return true;
        }

        public async Task<Result> DeleteCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default)
        {
            List<CriticalChoreDb> openCriticalChores = await _context.CriticalChores
                .Where(c => c.ChoreId == choreId && c.CompletedChoreId == null)
                .ToListAsync(cancellationToken);

            if (!openCriticalChores.Any())
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, "No open critical status found for this chore.", string.Format(LogMessages.EntityNotFound, nameof(CriticalChoreDb), choreId));
            }

            _context.CriticalChores.RemoveRange(openCriticalChores);
            await _context.SaveChangesAsync(cancellationToken);
            _cache.RemovePriotizedChores();
            return true;
        }

        private async Task<bool> HasOpenCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default)
        {
            return await _context.CriticalChores
                .AnyAsync(c => c.ChoreId == choreId && c.CompletedChoreId == null, cancellationToken);
        }

        public async Task MarkCriticalChoresAsCompletedAsync(Dictionary<int, int> choreIdToCompletedChoreIdMap, CancellationToken cancellationToken = default)
        {
            if (choreIdToCompletedChoreIdMap == null || !choreIdToCompletedChoreIdMap.Any())
            {
                return;
            }

            List<int> choreIds = choreIdToCompletedChoreIdMap.Keys.ToList();

            // Find all open critical chores for these chore IDs
            List<CriticalChoreDb> openCriticalChores = await _context.CriticalChores
                .Where(c => choreIds.Contains(c.ChoreId) && c.CompletedChoreId == null)
                .ToListAsync(cancellationToken);

            // Update each critical chore to link it to the completed chore
            foreach (CriticalChoreDb criticalChore in openCriticalChores)
            {
                if (choreIdToCompletedChoreIdMap.TryGetValue(criticalChore.ChoreId, out int completedChoreId))
                {
                    criticalChore.CompletedChoreId = completedChoreId;
                    criticalChore.UpdatedAt = _timeProvider.GetUtcNow();
                }
            }

            if (openCriticalChores.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

