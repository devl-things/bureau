namespace Sven.Data.Models
{
    public class ClientFeatureExternalRequirementDb
    {
        public int ClientId { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public string ExternalScopeKey { get; set; } = string.Empty;
        public ClientFeatureDb ClientFeature { get; set; } = null!;
    }
}
