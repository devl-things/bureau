using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("weeklyInterval")]
        public int WeeklyInterval { get; set; }

        [JsonPropertyName("isCritical")]
        public bool IsCritical { get; set; }
        [JsonPropertyName("criticalNote")]
        public string? CriticalNote { get; set; }
        [JsonPropertyName("criticalCreatedAt")]
        public DateOnly? CriticalCreatedAt { get; set; }

    }
}

