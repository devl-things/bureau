using System.Security.Claims;

namespace Sven.Models
{
    public class AuthCode
    {
        public string Code { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string CodeChallenge { get; set; } = string.Empty;

        public string CodeChallengeMethod { get; set; } = "S256";

        public string RedirectUri { get; set; } = string.Empty;

        public List<Claim> Claims { get; set; } = new List<Claim>();

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
