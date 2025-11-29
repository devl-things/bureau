using Niles.Chores;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;

namespace Niles.Chores.Api.Mappers
{
    public static class ChoreMapperExtension
    {
        public static ChoreDto ToDto(this Chore chore, bool isCritical = false, int priority = 0, bool completed = false)
        {
            return new ChoreDto
            {
                Id = IdObfuscator.Encode(chore.Id),
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Type = chore.Type.ToString(),
                WeeklyInterval = chore.WeeklyInterval,
                Priority = priority,
                Completed = completed,
                IsCritical = isCritical
            };
        }
    }
}

