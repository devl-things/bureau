using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.Text.Json.Serialization;

namespace Sven
{
    public class ClientRegistrationRequest
    {
        /// <summary>
        /// Array of redirection URI strings for use in redirect-based flows such as the authorization code and implicit flows.  As required by Section 2 of OAuth 2.0 [RFC6749], clients using flows with redirection MUST register their redirection URI values. Authorization servers that support dynamic registration for redirect-based flows MUST implement support for this metadata value
        /// </summary>
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.RedirectUris)]
        public List<string>? RedirectUris { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TokenEndpointAuthMethod)]
        public string? TokenEndpointAuthMethod { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.GrantTypesField)]
        public List<string>? GrantTypes { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ResponseTypesField)]
        public List<string>? ResponseTypes { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientName)]
        public string? ClientName { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.ClientUri)]
        public string? ClientUri { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.LogoUri)]
        public string? LogoUri { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.Scope)]
        public string? Scope { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.Contacts)]
        public List<string>? Contacts { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.TosUri)]
        public string? TosUri { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.PolicyUri)]
        public string? PolicyUri { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.JwksUri)]
        public string? JwksUri { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.Jwks)]
        public string? Jwks { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.SoftwareId)]
        public string? SoftwareId { get; set; }
        [FromBody]
        [JsonPropertyName(AuthConstants.OAuth.FieldNames.SoftwareVersion)]
        public string? SoftwareVersion { get; set; }
    }
}
