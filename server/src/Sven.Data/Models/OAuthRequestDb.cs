namespace Sven.Data.Models
{
    internal class OAuthRequestDb
    {
        public int Id { get; set; }
        public string PkceKey { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? RedirectUri { get; set; }
        public string? Scope { get; set; }
        public string? CodeChallenge { get; set; }
        public string? CodeChallengeMethod { get; set; }
        public string? State { get; set; }
        public string? Nonce { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
