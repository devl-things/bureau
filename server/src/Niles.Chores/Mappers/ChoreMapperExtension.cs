using Niles.Chores.Models;

namespace Niles.Chores.Mappers
{
    internal static class ChoreMapperExtension
    {
        internal static Chore ToChore(this ChoreDb choreDb)
        {
            return new Chore
            {
                Id = choreDb.Id,
                Title = choreDb.Title,
                Description = choreDb.Description,
                Type = choreDb.Type,
                WeeklyInterval = choreDb.WeeklyInterval
            };
        }
        internal static Chore ToChore(this ChoreDetail choreDetail)
        {
            return new Chore
            {
                Id = choreDetail.Chore.Id,
                Title = choreDetail.Chore.Title,
                Description = choreDetail.Chore.Description,
                Type = choreDetail.Chore.Type,
                WeeklyInterval = choreDetail.Chore.WeeklyInterval,
                IsCrititical = choreDetail.OpenCritical is not null,
                Note = choreDetail.OpenCritical?.Note,
                CriticalCreatedAt = choreDetail.OpenCritical?.CreatedAt,
            };
        }
    }
}

