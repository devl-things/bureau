using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;

namespace Sven.Models
{
    public class AuthorizeRequest
    {
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.ResponseTypeField)]
        public string ResponseType { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.ClientId)]
        public string ClientId { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.RedirectUri)]
        public string RedirectUri { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.Scope)]
        public string Scope { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.CodeChallenge)]
        public string CodeChallenge { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.CodeChallengeMethod)]
        public string CodeChallengeMethod { get; set; } = AuthConstants.OAuth.CodeChallengeMethods.Sha256;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.State)]
        public string State { get; set; } = string.Empty;
        [FromQuery(Name = AuthConstants.OAuth.FieldNames.Nonce)]
        public string? Nonce { get; set; }
    }
}
