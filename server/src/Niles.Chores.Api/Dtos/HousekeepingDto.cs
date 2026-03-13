using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class HousekeepingDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("datetime")]
        public string DateTime { get; set; } = string.Empty;
        [JsonPropertyName("duration")]
        public string? Duration { get; set; }
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("completedChores")]
        public List<ChoreDto> CompletedChores { get; set; } = new List<ChoreDto>();
    }
}

