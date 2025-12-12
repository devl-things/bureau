using Bureau;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class HousekeepingMapperExtension
    {
        public static HousekeepingDto ToDto(this Housekeeping housekeeping, Func<int, string> idFormatter)
        {
            return new HousekeepingDto
            {
                Id = housekeeping.Id.HasValue ? idFormatter(housekeeping.Id.Value) : null,
                DateTime = housekeeping.DateTime.UtcDateTime,
                Duration = housekeeping.Duration.ToString(),
                Note = housekeeping.Note,
                CompletedChoreIds = housekeeping.CompletedChoreIds ?? new List<int>(),
                CompletedChores = housekeeping.CompletedChores?.Select(c => c.ToDto(idFormatter)).ToList() ?? new List<ChoreDto>()
            };
        }

        public static Result<Housekeeping> ToResultModel(this HousekeepingDto dto)
        {
            Housekeeping model = new();
            try
            {
                model.DateTime = new DateTimeOffset(dto.DateTime);
            }
            catch (Exception ex)
            {
                return $"Invalid datetime: {ex.Message}";
            }
            // parse duration
            if (!ChoresDateParser.TryParseDuration(dto.Duration, out TimeSpan duration))
            {
                return "Duration should be set";
            }
            model.Duration = duration;

            model.Note = dto.Note;
            //TODO #82 this should be List<string>
            model.CompletedChoreIds = dto.CompletedChoreIds ?? new List<int>();
            return model;
        }
    }
}
