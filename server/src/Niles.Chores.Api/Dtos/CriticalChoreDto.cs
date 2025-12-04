using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class CriticalChoreDto
    {
        [JsonPropertyName("date")]
        [Required]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [Required]
        public string Description { get; set; } = string.Empty;
    }
}

