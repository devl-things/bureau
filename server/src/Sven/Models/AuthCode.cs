using Sven.Configurations;

namespace Sven.Models
{
    public class AuthCode : ClientClaims
    {
        public string Code { get; set; } = string.Empty;
        public string CodeChallenge { get; set; } = string.Empty;
        public string CodeChallengeMethod { get; set; } = AuthConstants.OAuth.CodeChallengeMethods.Sha256;
        public string RedirectUri { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
