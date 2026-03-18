namespace Sven.Data.Models
{
    internal class FailedExchangeAttemptDb
    {
        public string ClientId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int FailureCount { get; set; }
        public DateTimeOffset? LockoutStartedAt { get; set; }
    }
}
