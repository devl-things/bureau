namespace Bureau.Google.Calendar.Models
{
    public class CalendarListModel
    {
        public string LoginEmail { get; set; } = default!;
        public List<CalendarModel> Calendars { get; set; }
        public CalendarListModel()
        {
            Calendars = new List<CalendarModel>();
        }
    }
}
