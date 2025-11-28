using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreUpdateDto
    {
        [JsonPropertyName("title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("repeatEveryWeeks")]
        public int? WeeklyInterval { get; set; }
    }
}

