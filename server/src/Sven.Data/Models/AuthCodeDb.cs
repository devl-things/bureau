namespace Sven.Data.Models
{
    internal class AuthCodeDb
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string? CodeChallenge { get; set; }
        public string? CodeChallengeMethod { get; set; }
        public string? Nonce { get; set; }
        /// <summary>JSON-serialized List of (Type, Value) pairs</summary>
        public string Claims { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
