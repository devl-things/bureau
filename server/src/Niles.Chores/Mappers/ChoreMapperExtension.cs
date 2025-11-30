using Niles.Chores;
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
    }
}

