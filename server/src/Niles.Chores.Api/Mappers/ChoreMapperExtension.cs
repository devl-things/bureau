using Niles.Chores.Abstractions.Models;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;

namespace Niles.Chores.Api.Mappers
{
    public static class ChoreMapperExtension
    {
        public static ChoreDto ToDto(this Chore chore, int priority = 0, bool completed = false)
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
                IsCritical = chore.IsCrititical
            };
        }

        public static Result<Chore> ToResultModel(this ChoreDto dto)
        {
            Chore model = new Chore();
            // parse type
            if (!Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType type))
            {
                return new Result<Chore>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Invalid type. Expected one of: {string.Join(", ", Enum.GetNames<ChoreType>())}."
                };
            }
            model.Type = type;

            model.Title = dto.Title;
            model.Description = dto.Description;
            model.WeeklyInterval = dto.WeeklyInterval;
            return new Result<Chore>()
            {
                IsSuccess = true,
                Value = model
            };
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

