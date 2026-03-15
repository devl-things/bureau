namespace Sven.Data.Models
{
    internal class UserExternalTokenDb
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ExternalAccountId { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
        /// <summary>AES-256-GCM encrypted access token</summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>AES-256-GCM encrypted refresh token; null when provider doesn't issue one</summary>
        public string? RefreshToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset LinkedAt { get; set; }
        public DateTimeOffset? LastRefreshedAt { get; set; }
        public bool RequiresReauthorisation { get; set; }
    }
}
