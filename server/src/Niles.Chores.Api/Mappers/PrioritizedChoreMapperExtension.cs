using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class PrioritizedChoreMapperExtension
    {
        public static PrioritizedChoreDto ToDto(this PrioritizedChore chore, Func<int, string> idFormatter)
        {
            return new PrioritizedChoreDto
            {
                Id = idFormatter(chore.Id),
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Criticality = (int)chore.Criticality,
                Priority = chore.Priority,
                Type = chore.Type.ToString(),
                IsCompleted = false
            };
        }
    }
}
