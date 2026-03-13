using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class UpdateChoreRequest : ChoreRequestBase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
}
