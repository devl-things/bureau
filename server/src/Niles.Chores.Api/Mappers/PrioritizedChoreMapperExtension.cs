using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class PrioritizedChoreMapperExtension
    {
        public static PrioritizedChoreDto ToDto(this PrioritizedChore chore)
        {
            return new PrioritizedChoreDto
            {
                Id = chore.Id,
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
