using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class PrioritizedChoreService : IPrioritizedChoreService
    {
        const int THRESHOLD_FACTOR = 2;
        private readonly ChoresContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PrioritizedChoreService> _logger;
        private readonly TimeProvider _timeProvider;
        public PrioritizedChoreService(TimeProvider timeProvider, ILogger<PrioritizedChoreService> logger, ChoresContext context, IMemoryCache cache)
        {
            _timeProvider = timeProvider;
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<List<PrioritizedChore>> GetPrioritizedChoresAsync(DateOnly requestedDate, CancellationToken cancellationToken = default)
        {
            DateTime today = _timeProvider.GetUtcNow().UtcDateTime;
            if (requestedDate == default || requestedDate < DateOnly.FromDateTime(today))
            {
                _logger.LogError("Requested date ({requestedDate}) is either default or in the past from today ({today})", requestedDate, today);
                return [];
            }
            YearsWeek requestedYearWeek = new YearsWeek(requestedDate);
            if (_cache.TryGetPriotizedChores(requestedYearWeek, out List<PrioritizedChore> list))
            {
                return list;
            }

            IAsyncEnumerable<ChoreDetail> chores = _context.Chores
                .Select(chore => new ChoreDetail
                {
                    Chore = chore,
                    CompletedAt = chore.CompletedChores
                        .OrderByDescending(cc => cc.Housekeeping.Timestamp)
                        .Select(cc => cc.Housekeeping.Timestamp)
                        .FirstOrDefault(),
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
                .AsAsyncEnumerable();

            List<PrioritizedChore> prioritizedChores = [];
            await foreach (ChoreDetail chore in chores)
            {
                ChoreImportance importance = CalculateImportance(requestedYearWeek, chore);

                if (importance.Criticality == ChoreCriticality.None)
                {
                    // something went wrong in priority calculation
                    continue;
                }
                prioritizedChores.Add(new PrioritizedChore
                {
                    Id = chore.Chore.Id,
                    Title = chore.Chore.Title,
                    Description = chore.Chore.Description,
                    Criticality = importance.Criticality,
                    Priority = importance.Priority,
                    Type = chore.Chore.Type,
                    Note = chore.OpenCritical?.Note
                });
            }
            prioritizedChores = [.. prioritizedChores
                .OrderBy(pc => pc.Criticality)
                .ThenBy(pc => pc.Priority)];

            _cache.SetPriotizedChores(requestedYearWeek, prioritizedChores);

            return prioritizedChores;
        }

        internal ChoreImportance CalculateImportance(YearsWeek currentYearWeek, ChoreDetail chore)
        {
            if (chore.OpenCritical is not null)
            {
                return new ChoreImportance() { Criticality = ChoreCriticality.Critical };
            }
            if (chore.CompletedAt is null)
            {
                return new ChoreImportance() { Criticality = ChoreCriticality.High, Priority = chore.Score };
            }

            // Calculate time difference between the provided date and the last completed date in weeks
            YearsWeek completedYearsWeek = new YearsWeek(chore.CompletedAt.Value);
            int weekDiff = currentYearWeek - completedYearsWeek;

            if (weekDiff < 0)
            {
                _logger.LogError("Week diff ({weekDiff}) is negative. currentYearWeek = {currentYearWeek} | completedYearsWeek = {completedYearsWeek})", weekDiff, currentYearWeek, completedYearsWeek);
                return new ChoreImportance() { Criticality = ChoreCriticality.None };
            }
            else if (weekDiff < chore.Chore.WeeklyInterval)
            {
                return new ChoreImportance() { Criticality = ChoreCriticality.Low, Priority = chore.Score };
            }
            else if (weekDiff <= (chore.Chore.WeeklyInterval * THRESHOLD_FACTOR))
            {
                return new ChoreImportance() { Criticality = ChoreCriticality.Medium, Priority = chore.Score };
            }
            else if (weekDiff > (chore.Chore.WeeklyInterval * THRESHOLD_FACTOR))
            {
                return new ChoreImportance() { Criticality = ChoreCriticality.High, Priority = chore.Score };
            }
            _logger.LogCritical("This is argmagedon or I didn't see something.");
            return new ChoreImportance() { Criticality = ChoreCriticality.None };
        }
        internal struct ChoreImportance
        {
            public ChoreCriticality Criticality { get; set; }
            public int Priority { get; set; }
        }
    }
}
