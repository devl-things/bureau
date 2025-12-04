using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class HousekeepingSubmitDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;
        [JsonPropertyName("duration")]
        public int? Duration { get; set; }
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("completedChoreIds")]
        public List<string> CompletedChoreIds { get; set; } = new List<string>();
    }
}

