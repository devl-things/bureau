using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class TokenExchangeResponse
    {
        [JsonPropertyName("access_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expires_in")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("issued_token_type")]
        public string IssuedTokenType { get; set; } = "urn:ietf:params:oauth:token-type:access_token";

        [JsonPropertyName("bureau_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<BureauTokenEntry>? BureauTokens { get; set; }
    }
}
