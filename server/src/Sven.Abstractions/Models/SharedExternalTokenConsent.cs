namespace Sven
{
    public class SharedExternalTokenConsent
    {
        public string Identifier { get; set; } = string.Empty;
        public string HouseholdId { get; set; } = string.Empty;
        public string OwnerUserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string FeatureKey { get; set; } = string.Empty;
        public DateTimeOffset SharedAt { get; set; }
    }
}
