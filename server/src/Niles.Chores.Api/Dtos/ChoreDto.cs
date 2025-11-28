using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("title")]
        [Required]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("type")]
        [Required]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("weeklyInterval")]
        [Required]
        public int WeeklyInterval { get; set; }
        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
        [JsonPropertyName("isCritical")]
        public bool IsCritical { get; set; }
    }
}
