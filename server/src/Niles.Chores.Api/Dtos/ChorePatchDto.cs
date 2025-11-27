using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChorePatchDto
    {
        [JsonPropertyName("completed")]
        public bool IsCompleted { get; set; }
        [JsonPropertyName("date")]
        public DateOnly Date { get; set; }
    }
}
