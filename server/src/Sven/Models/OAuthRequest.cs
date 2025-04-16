namespace Sven.Models
{
    public class OAuthRequest
    {
        public string ClientId { get; set; } = string.Empty;

        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;

        public string CodeChallenge { get; set; } = string.Empty;

        public string CodeChallengeMethod { get; set; } = "S256";

        public string State { get; set; } = string.Empty;
    }
}
