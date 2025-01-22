using Bureau.Google.Calendar.Data;
using Bureau.Google.Calendar.Models;

namespace Bureau.Google.Calendar.Mappers
{
    internal static class CalendarListMapper
    {
        internal static CalendarListModel ToModel(this CalendarList data)
        {

            CalendarListModel model = new CalendarListModel();
            foreach (CalendarListEntry item in data.Items)
            {
                model.Calendars.Add(item.ToModel());
            }  
            return model;
        }
    }
}
