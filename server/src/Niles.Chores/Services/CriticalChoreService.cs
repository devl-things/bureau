using Microsoft.EntityFrameworkCore;
using Niles.Chores;
using Niles.Chores.Abstractions.Models;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class CriticalChoreService : ICriticalChoreService
    {
        private readonly ChoresContext _context;

        public CriticalChoreService(ChoresContext context)
        {
            _context = context;
        }

        public async Task<Result> CreateCriticalChoreAsync(int choreId, string note, CancellationToken cancellationToken = default)
        {
            // Verify chore exists
            var choreExists = await _context.Chores.AnyAsync(c => c.Id == choreId, cancellationToken);
            if (!choreExists)
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chore with Id {choreId} not found"
                };
            }

            // Check if there's already an open critical chore for this chore
            var existingOpen = await HasOpenCriticalChoreAsync(choreId, cancellationToken);
            if (existingOpen)
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = "This chore already has an open critical status. Please remove it first."
                };
            }

            var criticalChore = new CriticalChoreDb
            {
                ChoreId = choreId,
                Note = note,
                CompletedChoreId = null, // Not completed yet
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.CriticalChores.Add(criticalChore);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result
            {
                IsSuccess = true
            };
        }

        public async Task<Result> DeleteCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default)
        {
            // Find all open critical chores for this chore
            var openCriticalChores = await _context.CriticalChores
                .Where(c => c.ChoreId == choreId && c.CompletedChoreId == null)
                .ToListAsync(cancellationToken);

            if (!openCriticalChores.Any())
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = "No open critical status found for this chore."
                };
            }

            _context.CriticalChores.RemoveRange(openCriticalChores);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result
            {
                IsSuccess = true
            };
        }

        public async Task<bool> HasOpenCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default)
        {
            return await _context.CriticalChores
                .AnyAsync(c => c.ChoreId == choreId && c.CompletedChoreId == null, cancellationToken);
        }

        public async Task<List<int>> GetOpenCriticalChoreIdsAsync(List<int> choreIds, CancellationToken cancellationToken = default)
        {
            if (choreIds == null || !choreIds.Any())
            {
                return new List<int>();
            }

            return await _context.CriticalChores
                .Where(c => choreIds.Contains(c.ChoreId) && c.CompletedChoreId == null)
                .Select(c => c.ChoreId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public async Task MarkCriticalChoresAsCompletedAsync(Dictionary<int, int> choreIdToCompletedChoreIdMap, CancellationToken cancellationToken = default)
        {
            if (choreIdToCompletedChoreIdMap == null || !choreIdToCompletedChoreIdMap.Any())
            {
                return;
            }

            var choreIds = choreIdToCompletedChoreIdMap.Keys.ToList();

            // Find all open critical chores for these chore IDs
            var openCriticalChores = await _context.CriticalChores
                .Where(c => choreIds.Contains(c.ChoreId) && c.CompletedChoreId == null)
                .ToListAsync(cancellationToken);

            // Update each critical chore to link it to the completed chore
            foreach (var criticalChore in openCriticalChores)
            {
                if (choreIdToCompletedChoreIdMap.TryGetValue(criticalChore.ChoreId, out int completedChoreId))
                {
                    criticalChore.CompletedChoreId = completedChoreId;
                    criticalChore.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            if (openCriticalChores.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

