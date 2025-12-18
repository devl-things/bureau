using Bureau;
using Bureau.Primitives.Errors;
using Bureau.Server.Contracts;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Mappers
{
    public static class HousekeepingMapperExtension
    {
        public static HousekeepingDto ToDto(this Housekeeping housekeeping, IIdObfuscator idObfuscator)
        {
            return new HousekeepingDto
            {
                Id = idObfuscator.Encode(housekeeping.Id),
                DateTime = housekeeping.DateTime.ToString(),
                Duration = housekeeping.Duration.ToString(),
                Note = housekeeping.Note,
                CompletedChores = housekeeping.CompletedChores?.Select(c => c.ToDto(idObfuscator)).ToList() ?? new List<ChoreDto>()
            };
        }

        public static Result<Housekeeping> ToResultModel(this CreateHousekeepingRequest dto, IIdObfuscator idObfuscator)
        {
            return dto.ToResultModelBase(idObfuscator);
        }
        public static Result<Housekeeping> ToResultModel(this UpdateHousekeepingRequest dto, IIdObfuscator idObfuscator)
        {
            Result<Housekeeping> housekeepingResult = dto.ToResultModelBase(idObfuscator);
            if (housekeepingResult.IsError)
            {
                return housekeepingResult;
            }

            Result<int> idResult = idObfuscator.Decode(dto.Id);
            if (idResult.IsError)
            {
                return idResult.Error;
            }
            Housekeeping housekeeping = housekeepingResult.Value;
            housekeeping.Id = idResult.Value;
            return housekeeping;
        }

        private static Result<Housekeeping> ToResultModelBase(this HousekeepingRequestBase dto, IIdObfuscator idObfuscator)
        {
            if (!ChoresDateParser.TryParseDateTime(dto.DateTime, out DateTimeOffset dateTime))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, "Invalid datetime", $"DateTime malformed, passed value {dto.DateTime}");
            }
            if (!ChoresDateParser.TryParseDuration(dto.Duration, out TimeSpan duration))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, "Invalid duration", $"Duration malformed, passed value {dto.Duration}");
            }
            if (duration == TimeSpan.Zero)
            {
                return ResultError.From(ProblemCodes.Validation.OutOfRange, "Duration cannot be zero");
            }

            List<int> completedChoreIds = new List<int>(dto.CompletedChoreIds.Count);
            foreach (string id in dto.CompletedChoreIds)
            {
                Result<int> idResult = idObfuscator.Decode(id);
                if (idResult.IsSuccess)
                {
                    completedChoreIds.Add(idResult.Value);
                }
                else
                {
                    return idResult.Error;
                }
            }

            Housekeeping model = new Housekeeping
            {
                DateTime = dateTime,
                Duration = duration,
                Note = dto.Note,
                CompletedChoreIds = completedChoreIds
            };

            return model;
        }
    }
}
