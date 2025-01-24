using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Calendar.Models
{
    public class CalendarRequestModel
    {
        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}
