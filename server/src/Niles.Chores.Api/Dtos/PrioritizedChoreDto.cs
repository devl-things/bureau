using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class PrioritizedChoreDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("criticality")]
        public int Criticality { get; set; }
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("completed")]
        public bool IsCompleted { get; set; }
        [JsonIgnore]
        public DateOnly Date { get; set; }
    }

}
