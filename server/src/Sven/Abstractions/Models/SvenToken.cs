using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class SvenToken
    {
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.AccessToken)]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.RefreshToken)]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenType)]
        public string TokenType { get; set; } = "Bearer";
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ExpiresIn)]
        public int ExpiresIn { get; set; } = 3600;

        public SvenToken(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
