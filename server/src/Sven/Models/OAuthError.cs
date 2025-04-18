using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class OAuthError
    {
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.Error)]
        public string Error { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ErrorDescription)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorDescription { get; set; }

        public OAuthError() { }
        public OAuthError(string error, string? errorDescription)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }
    }
}
