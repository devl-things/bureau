using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreSubmissionDto
    {
        [JsonPropertyName("date")]
        [Required]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public double? DurationMinutes { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("completedChoreIds")]
        public List<string> CompletedChoreIds { get; set; } = new();
    }
}

