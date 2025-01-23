using Bureau.Calendar.Models;
using Bureau.Recipes.Models;
using Bureau.UI.API.V1.Models.Calendar;

namespace Bureau.UI.API.V1.Mappers
{
    internal static class CalendarModelMapper
    {
        internal static CalendarDto ToDto(this CalendarRequestModel data)
        {
            return new CalendarDto()
            {
                Name = data.Name,
                Description = data.Description
            };
        }

        internal static CalendarDto ToDto(this CalendarRequestModel data, string id)
        {
            CalendarDto result = data.ToDto();
            result.Id = id;
            return result;
        }
        internal static CalendarResponseModel ToResponseModel(this CalendarDto data)
        {
            return new CalendarResponseModel()
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt
            };
        }
    }
}
