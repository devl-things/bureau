namespace Sven.Data.Models
{
    /// <summary>
    /// Consent record: OwnerUserId agrees to share their external token
    /// for Provider+FeatureKey with all members of HouseholdIdentifier.
    /// </summary>
    internal class SharedExternalTokenDb
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string HouseholdIdentifier { get; set; } = string.Empty;
        public string OwnerUserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string FeatureKey { get; set; } = string.Empty;
        public DateTimeOffset SharedAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
    }
}
