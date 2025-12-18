using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public abstract class HousekeepingRequestBase
    {
        [JsonPropertyName("datetime")]
        public string DateTime { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("completedChoreIds")]
        public List<string> CompletedChoreIds { get; set; } = new();
    }
}
