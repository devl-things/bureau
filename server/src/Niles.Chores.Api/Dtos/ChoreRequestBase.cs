using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreRequestBase
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("weeklyInterval")]
        public int WeeklyInterval { get; set; }
    }
}
