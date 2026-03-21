namespace Sven.Configurations
{
    internal class TokenExchangeOptions
    {
        public const string SectionName = "TokenExchange";

        public int MaxFailuresBeforeLockout { get; set; } = 5;
        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
    }
}
