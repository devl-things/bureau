namespace Bureau.Calendar.Models
{
    public class CalendarDto
    {
        public string Id { get; set; } = string.Empty;
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public static CalendarDto EmptyCalendar()
        {
            return new CalendarDto()
            {
                Id = string.Empty,
                Name = string.Empty,
            };
        }
    }
}
