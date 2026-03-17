using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public class TokenRequest
    {
        [FromForm(Name = AuthConstants.OAuth.FieldNames.GrantTypeField)]
        [Required]
        public string GrantType { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.ClientId)]
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.RedirectUri)]
        public string? RedirectUri { get; set; }
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Code)]
        public string? Code { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.CodeVerifier)]
        public string? CodeVerifier { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.RefreshToken)]
        public string? RefreshToken { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Scope)]
        public string? Scope { get; set; }
        [FromForm(Name = AuthConstants.OAuth.FieldNames.ClientSecret)]
        public string? ClientSecret { get; set; }
        [FromForm(Name = "subject_token")]
        public string? SubjectToken { get; set; }
        [FromForm(Name = "subject_token_type")]
        public string? SubjectTokenType { get; set; }
        [FromForm(Name = "resource")]
        public string? Resource { get; set; }
    }
}
