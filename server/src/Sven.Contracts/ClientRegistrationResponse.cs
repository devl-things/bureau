using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven
{
    public class ClientRegistrationResponse : ClientRegistrationRequest
    {
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientId)]
        public string ClientId { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientSecret)]
        public string? ClientSecret { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientIdIssuedAt)]
        public DateTimeOffset? ClientIdIssuedAt { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientSecretExpiresAt)]
        public DateTimeOffset? ClientSecretExpiresAt { get; set; }
    }
}
