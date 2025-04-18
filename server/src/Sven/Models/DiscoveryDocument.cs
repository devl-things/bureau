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

        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ResponseTypesSupported)]
        public string[] ResponseTypesSupported { get; set; } = new[] { AuthConstants.OAuth.ResponseTypes.Code };
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.SubjectTypesSupported)]
        public string[] SubjectTypesSupported { get; set; } = new[] { AuthConstants.OAuth.SubjectTypes.Public };
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.IdTokenSigningAlgValuesSupported)]
        public string[] IdTokenSigningAlgValuesSupported { get; set; } = new[] { AuthConstants.OAuth.SigningAlgorithms.Rsa256 };
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenEndpointAuthMethodsSupported)]
        public string[] TokenEndpointAuthMethodsSupported { get; set; } = new[] { AuthConstants.OAuth.TokenAuthMethods.None };
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.CodeChallengeMethodsSupported)]
        public string[] CodeChallengeMethodsSupported { get; set; } = new[] { AuthConstants.OAuth.CodeChallengeMethods.Sha256 };

        // Optional:
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ScopesSupported)]
        public string[] ScopesSupported { get; set; } = new[] {
            AuthConstants.Scopes.OpenId, AuthConstants.Scopes.Profile, AuthConstants.Scopes.Email,
            AuthConstants.Scopes.Phone, AuthConstants.Scopes.Address, AuthConstants.Scopes.OfflineAccess
        };
    }
}
