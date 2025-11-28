using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Niles.Chores.Api.Dtos
{
    public class HousekeepingDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }
        [JsonPropertyName("duration")]
        [Required]
        public string Duration { get; set; } = string.Empty;
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("completedChoreIds")]
        public List<int> CompletedChoreIds { get; set; } = new List<int>();
    }
}
