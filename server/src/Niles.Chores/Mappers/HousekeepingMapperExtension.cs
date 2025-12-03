using Niles.Chores;
using Niles.Chores.Models;

namespace Niles.Chores.Mappers
{
    internal static class HousekeepingMapperExtension
    {
        internal static Housekeeping ToHousekeeping(this HousekeepingDb housekeepingDb)
        {
            return new Housekeeping
            {
                Id = housekeepingDb.Id,
                DateTime = housekeepingDb.Timestamp,
                Duration = housekeepingDb.Duration,
                Note = housekeepingDb.Note,
                CompletedChoreIds = housekeepingDb.CompletedChores.Select(c => c.ChoreId).ToList(),
                CompletedChores = housekeepingDb.CompletedChores
                    .Select(cc => cc.Chore.ToChore())
                    .ToList()
            };
        }
    }
}

