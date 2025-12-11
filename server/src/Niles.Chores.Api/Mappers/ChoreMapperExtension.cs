using Bureau;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;

namespace Niles.Chores.Api.Mappers
{
    public static class ChoreMapperExtension
    {
        //TODO #72 [backend] change so this obfuscator is passed as formating option
        //TODO #74 [backend] set priority from chore
        public static ChoreDto ToDto(this Chore chore)
        {
            return new ChoreDto
            {
                Id = IdObfuscator.Encode(chore.Id),
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
                return $"Invalid type. Expected one of: {string.Join(", ", Enum.GetNames<ChoreType>())}.";
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
            if (!result.IsSuccess)
            {
                return result;
            }
            result.Value!.Id = id;
            return result;
        }
    }
}

