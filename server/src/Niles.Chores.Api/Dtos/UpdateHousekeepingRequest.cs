using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class UpdateHousekeepingRequest : HousekeepingRequestBase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
}
