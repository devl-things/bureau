using Bureau.Google.Calendar.Data;
using Bureau.Google.Calendar.Models;

namespace Bureau.Google.Calendar.Mappers
{
    internal static class CalendarListEntryMapper
    {
        internal static CalendarModel ToModel(this CalendarListEntry data) 
        {
            CalendarModel model = new CalendarModel();
            model.Id = data.Id;
            model.Name = data.ETag;
            model.Description = data.Description;
            model.Summary = data.Summary;
            model.BackgroundColor = data.BackgroundColor;

            return model;
        }
    }
}
