namespace Bureau.Core.Models
{
    public interface IEntryReference : IReference
    {
        public string ProviderName { get; }

        public string? ExternalId { get; }
    }
}
