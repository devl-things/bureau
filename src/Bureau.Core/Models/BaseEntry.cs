using Bureau.Core.Constants;

namespace Bureau.Core.Models
{
    public class BaseEntry : BaseRecord, IEntry
    {
        public string ProviderName { get; set; } = BureauConstants.BureauProvider;

        public string? ExternalId { get; set; }
    }
}
