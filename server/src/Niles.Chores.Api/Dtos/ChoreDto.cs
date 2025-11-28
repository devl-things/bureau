using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("completed")]
        public bool IsCompleted { get; set; }
        [JsonIgnore]
        public DateOnly Date { get; set; }
    }

}
