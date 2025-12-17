using Bureau;
using Bureau.Primitives.Errors;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class ChoreMapperExtension
    {
        //TODO #84 [backend] set priority from chore
        public static ChoreDto ToDto(this Chore chore, Func<int, string> idFormatter)
        {
            return new ChoreDto
            {
                Id = idFormatter(chore.Id),
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Type = chore.Type.ToString(),
                WeeklyInterval = chore.WeeklyInterval,
                IsCritical = chore.IsCrititical,
                CriticalNote = chore.Note,
                CriticalCreatedAt = chore.IsCrititical ? DateOnly.FromDateTime(chore.CriticalCreatedAt!.Value.DateTime) : null,
            };
        }

        public static Result<Chore> ToResultModel(this ChoreDto dto)
        {
            Chore model = new Chore();
            // parse type
            if (!Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType type))
            {
                return new ResultError(ProblemCodes.Validation.Failed, $"Invalid type.", null!, $"Received {dto.Type}, expected one of: {string.Join(", ", Enum.GetNames<ChoreType>())}.");
            }
            model.Type = type;

            model.Title = dto.Title;
            model.Description = dto.Description;
            model.WeeklyInterval = dto.WeeklyInterval;
            return model;
        }
        public static Result<Chore> ToResultModel(this ChoreDto dto, int id)
        {
            Result<Chore> result = dto.ToResultModel();
            if (result.IsError)
            {
                return result;
            }
            result.Value!.Id = id;
            return result;
        }
    }
}

