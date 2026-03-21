namespace Sven
{
    public class UserExternalToken
    {
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ExternalAccountId { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset LinkedAt { get; set; }
        public DateTimeOffset? LastRefreshedAt { get; set; }
        public bool RequiresReauthorisation { get; set; }
    }
}
