using System.Collections.Generic;

namespace Sven.Data.Models
{
    public class ClientFeatureDb
    {
        public int ClientId { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public ClientDb Client { get; set; } = null!;
        public ICollection<ClientFeatureExternalRequirementDb> ExternalRequirements { get; set; } = null!;
    }
}
