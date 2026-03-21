using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class SvenToken
    {
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.AccessToken)]
        public string AccessToken { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.RefreshToken)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.IdToken)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IdToken { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenType)]
        public string TokenType { get; set; } = "Bearer";
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ExpiresIn)]
        public int ExpiresIn { get; set; } = 3600;

        public SvenToken(string accessToken, string? refreshToken, string? idToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            IdToken = idToken;
        }
    }
}
