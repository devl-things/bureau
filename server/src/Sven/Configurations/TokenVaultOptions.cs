namespace Sven.Configurations
{
    public class TokenVaultOptions
    {
        public int RefreshIntervalSeconds { get; set; } = 300;
        public int RefreshLeadTimeMinutes { get; set; } = 15;
    }
}
