using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven.Models
{
    public class DiscoveryDocument
    {
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.Issuer)]
        public string Issuer { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.AuthorizationEndpoint)]
        public string AuthorizationEndpoint { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenEndpoint)]
        public string TokenEndpoint { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.UserInfoEndpoint)]
        public string UserInfoEndpoint { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.JwksUri)]
        public string JwksUri { get; set; } = string.Empty;
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.IntrospectionEndpoint)]
        public string IntrospectionEndpoint { get; set; } = string.Empty;
        [JsonPropertyName("end_session_endpoint")]
        public string EndSessionEndpoint { get; set; } = string.Empty;

        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ResponseTypesSupported)]
        public List<string>? ResponseTypesSupported { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.SubjectTypesSupported)]
        public List<string>? SubjectTypesSupported { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.IdTokenSigningAlgValuesSupported)]
        public List<string>? IdTokenSigningAlgValuesSupported { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenEndpointAuthMethodsSupported)]
        public List<string>? TokenEndpointAuthMethodsSupported { get; set; }
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.CodeChallengeMethodsSupported)]
        public List<string>? CodeChallengeMethodsSupported { get; set; }

        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ScopesSupported)]
        public List<string>? ScopesSupported { get; set; }
    }
}
