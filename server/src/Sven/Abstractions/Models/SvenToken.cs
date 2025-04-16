using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class SvenToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } = 3600;

        public SvenToken(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
