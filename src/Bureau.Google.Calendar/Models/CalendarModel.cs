namespace Bureau.Google.Calendar.Models
{
    public class CalendarModel
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;

        public string BackgroundColor { get; set; }
        public string? Description { get; set; } = default!;
        public virtual string Summary { get; set; }
    }
}
