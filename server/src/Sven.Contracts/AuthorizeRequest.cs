using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven
{
    public class AuthorizeRequest
    {
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.ResponseTypeField)]
        [Required]
        public string ResponseType { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.ClientId)]
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.RedirectUri)]
        [Required]
        public string RedirectUri { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.Scope)]
        public string Scope { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.CodeChallenge)]
        public string CodeChallenge { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.CodeChallengeMethod)]
        public string? CodeChallengeMethod { get; set; }
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.State)]
        public string? State { get; set; }
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.Nonce)]
        public string? Nonce { get; set; }
    }
}
