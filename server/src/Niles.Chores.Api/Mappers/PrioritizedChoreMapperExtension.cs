using Bureau.Server.Contracts;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class PrioritizedChoreMapperExtension
    {
        public static PrioritizedChoreDto ToDto(this PrioritizedChore chore, IIdObfuscator idObfuscator)
        {
            return new PrioritizedChoreDto
            {
                Id = idObfuscator.Encode(chore.Id),
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
