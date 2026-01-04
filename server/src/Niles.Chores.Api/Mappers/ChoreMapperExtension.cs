using Bureau;
using Bureau.Primitives.Errors;
using Bureau.Server.Contracts;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class ChoreMapperExtension
    {
        //TODO #84 [backend] set priority from chore
        public static ChoreDto ToDto(this Chore chore, IIdObfuscator idObfuscator)
        {
            return new ChoreDto
            {
                Id = idObfuscator.Encode(chore.Id),
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Type = chore.Type.ToString(),
                WeeklyInterval = chore.WeeklyInterval,
                IsCritical = chore.IsCrititical,
                CriticalNote = chore.Note,
                CriticalCreatedAt = chore.IsCrititical ? DateOnly.FromDateTime(chore.CriticalCreatedAt!.Value.DateTime) : null,
            };
        }

        public static Result<Chore> ToResultModel(this CreateChoreRequest dto)
        {
            return dto.ToResultModelBase();
        }
        public static Result<Chore> ToResultModel(this UpdateChoreRequest dto, IIdObfuscator idObfuscator)
        {
            Result<Chore> result = dto.ToResultModelBase();
            if (result.IsError)
            {
                return result;
            }
            Result<int> idResult = idObfuscator.Decode(dto.Id);
            if (idResult.IsError)
            {
                return idResult.Error;
            }
            Chore chore = result.Value;
            chore.Id = idResult.Value;
            return chore;
        }

        private static Result<Chore> ToResultModelBase(this ChoreRequestBase dto)
        {
            Chore model = new Chore();
            if (!Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType type))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidEnumValue, $"Invalid type.", $"Received {dto.Type}, expected one of: {string.Join(", ", Enum.GetNames<ChoreType>())}.");
            }
            model.Type = type;

            model.Title = dto.Title;
            model.Description = dto.Description;
            model.WeeklyInterval = dto.WeeklyInterval;
            return model;
        }
    }
}

