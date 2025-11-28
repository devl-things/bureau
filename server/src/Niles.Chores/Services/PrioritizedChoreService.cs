using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class PrioritizedChoreService : IPrioritizedChoreService
    {
        const int CRITICAL_PRIORITY = 1;
        const int HIGH_PRIORITY_FACTOR = 2;
        const int MEDIUM_PRIORITY_FACTOR = 3;
        const int LOW_PRIORITY_FACTOR = 4;
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
            // Allow today and future dates, reject past dates
            if (requestedDate == default)
            {
                _logger.LogWarning("Requested date is default");
                return [];
            }
            
            DateTime utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            DateOnly today = DateOnly.FromDateTime(utcNow);
            
            // Allow today and future dates
            if (requestedDate < today)
            {
                _logger.LogWarning("Requested date ({requestedDate}) is in the past from today ({today})", requestedDate, today);
                return [];
            }
            YearsWeek requestedYearWeek = new YearsWeek(requestedDate);
            if (_cache.TryGetValue<List<PrioritizedChore>>(requestedYearWeek, out List<PrioritizedChore>? list))
            {
                if (list is not null)
                {
                    return list;
                }
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
                int priority = CalculatePriority(requestedYearWeek, chore);

                if (priority <= 0)
                {
                    // something went wrong in priority calculation
                    continue;
                }
                prioritizedChores.Add(new PrioritizedChore
                {
                    Id = chore.Chore.Id,
                    Title = chore.Chore.Title,
                    Description = chore.Chore.Description,
                    Priority = priority,
                    Type = chore.Chore.Type,
                    WeeklyInterval = chore.Chore.WeeklyInterval,
                    Note = chore.OpenCritical?.Note
                });
            }
            
            // Order by priority (lower number = higher priority: 1=critical, then 2-4 factors, then higher numbers)
            // Within same priority level, order by title for consistency
            prioritizedChores = prioritizedChores
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.Title)
                .ToList();
            
            return prioritizedChores;
        }

        internal int CalculatePriority(YearsWeek currentYearWeek, ChoreDetail chore)
        {
            if (chore.OpenCritical is not null)
            {
                return CRITICAL_PRIORITY;
            }
            if (chore.CompletedAt is null)
            {
                return HIGH_PRIORITY_FACTOR * chore.ChoreImportanceScore;
            }

            // Calculate time difference between the provided date and the last completed date in weeks
            YearsWeek completedYearsWeek = new YearsWeek(chore.CompletedAt.Value);
            int weekDiff = currentYearWeek - completedYearsWeek;

            if (weekDiff < 0)
            {
                _logger.LogError("Week diff ({weekDiff}) is negative. currentYearWeek = {currentYearWeek} | completedYearsWeek = {completedYearsWeek})", weekDiff, currentYearWeek, completedYearsWeek);
                return -1;
            }
            else if (weekDiff < chore.Chore.WeeklyInterval)
            {
                return LOW_PRIORITY_FACTOR * chore.ChoreImportanceScore;
            }
            else if (weekDiff <= (chore.Chore.WeeklyInterval * 2))
            {
                return MEDIUM_PRIORITY_FACTOR * chore.ChoreImportanceScore;
            }
            else if (weekDiff > (chore.Chore.WeeklyInterval * 2))
            {
                return HIGH_PRIORITY_FACTOR * chore.ChoreImportanceScore;
            }
            _logger.LogCritical("This is argmagedon or I didn't see something.");
            return -1;
        }
    }
}
