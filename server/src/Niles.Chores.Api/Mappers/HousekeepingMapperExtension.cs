using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;

namespace Niles.Chores.Api.Mappers
{
    public static class HousekeepingMapperExtension
    {
        public static HousekeepingDto ToDto(this Housekeeping housekeeping)
        {
            return new HousekeepingDto
            {
                //TODO change so this obfuscator is passed as formating option
                Id = housekeeping.Id.HasValue ? IdObfuscator.Encode(housekeeping.Id.Value) : null,
                DateTime = housekeeping.DateTime.UtcDateTime,
                Duration = housekeeping.Duration.ToString(),
                Note = housekeeping.Note,
                CompletedChoreIds = housekeeping.CompletedChoreIds ?? new List<int>(),
                CompletedChores = housekeeping.CompletedChores?.Select(c => c.ToDto()).ToList() ?? new List<ChoreDto>()
            };
        }
    }
}
