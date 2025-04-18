using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;

namespace Sven.Models
{
    public class TokenRequest
    {
        [FromForm(Name = AuthConstants.OAuth.FieldNames.GrantTypeField)]
        public string GrantType { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.ClientId)]
        public string ClientId { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Code)]
        public string? Code { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.CodeVerifier)]
        public string? CodeVerifier { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.RefreshToken)]
        public string? RefreshToken { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Scope)]
        public string? Scope { get; set; }
    }
}
