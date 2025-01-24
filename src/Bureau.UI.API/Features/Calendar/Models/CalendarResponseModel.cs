namespace Bureau.UI.API.Features.Calendar.Models
{
    public class CalendarResponseModel : CalendarRequestModel
    {
        public required string Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
