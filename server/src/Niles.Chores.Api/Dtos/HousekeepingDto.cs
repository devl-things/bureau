using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        //TODO #82 this should be List<string>
        [JsonPropertyName("completedChoreIds")]
        public List<int> CompletedChoreIds { get; set; } = new List<int>();
        [JsonPropertyName("completedChores")]
        public List<ChoreDto> CompletedChores { get; set; } = new List<ChoreDto>();
    }
}
