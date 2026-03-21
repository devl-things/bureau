using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class BureauTokenEntry
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("source_user_id")]
        public string SourceUserId { get; set; } = string.Empty;

        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;
    }
}
