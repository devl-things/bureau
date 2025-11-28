using Niles.Chores.Abstractions.Services;

namespace Niles.Chores.Services
{
    public sealed class PrioritizedChoreService : IPrioritizedChoreService
    {
        private static readonly DateOnly ReferenceWeek = new(2024, 1, 1);

        private static readonly IReadOnlyList<ChoreTemplate> Templates = new List<ChoreTemplate>
        {
            new(101, "Kitchen deep clean", "Counters, stove, fridge exterior", ChoreType.Maintenance, WeeklyInterval: 2, Priority: 1),
            new(102, "Bathroom reset", "Scrub tiles, replace towels", ChoreType.Maintenance, 1, 2),
            new(103, "Dust + vacuum lounge", "Dust shelves, vacuum rug", ChoreType.Maintenance, 1, 3),
            new(104, "Laundry rotation", "Whites and delicates", ChoreType.Maintenance, 1, 4),
            new(105, "Plants & garden check", "Water and trim herbs", ChoreType.Extra, 2, 3),
            new(106, "Pantry tidy-up", "Re-stack pantry, check expiry dates", ChoreType.Extra, 4, 4),
            new(107, "Robot vacuum maintenance", "Empty bin, clean sensors", ChoreType.Maintenance, 3, 3),
            new(108, "Guest room reset", "Change bedding, dust side tables", ChoreType.Maintenance, 4, 2)
        };

        public Task<List<PrioritizedChore>> GetPrioritizedChoresAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            IEnumerable<PrioritizedChore> result = Templates
                .Where(template => ShouldSchedule(template, date))
                .Select(template => template.ToInstance());

            List<PrioritizedChore> chores = result
                .OrderBy(chore => chore.Priority)
                .ThenBy(chore => chore.Id)
                .ToList();

            if (!chores.Any())
            {
                chores = Templates
                    .OrderBy(t => t.Priority)
                    .Take(5)
                    .Select(t => t.ToInstance())
                    .ToList();
            }

            return Task.FromResult(chores);
        }

        private static bool ShouldSchedule(ChoreTemplate template, DateOnly date)
        {
            if (template.WeeklyInterval <= 1)
            {
                return true;
            }

            int weeksSinceReference = Math.Max(0, (date.DayNumber - ReferenceWeek.DayNumber) / 7);
            return weeksSinceReference % template.WeeklyInterval == 0;
        }

        private sealed record ChoreTemplate(
            int Id,
            string Title,
            string Description,
            ChoreType Type,
            int WeeklyInterval,
            int Priority)
        {
            public PrioritizedChore ToInstance()
            {
                return new PrioritizedChore
                {
                    Id = Id,
                    Title = Title,
                    Description = Description,
                    Type = Type,
                    WeeklyInterval = WeeklyInterval,
                    Priority = Priority
                };
            }
        }
    }
}
