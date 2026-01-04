namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class AttributeDto
    {
        public required string Key { get; init; }
        public string? Locale { get; init; }
        public required AttributeValueTypeContract Type { get; init; }

        // Exactly one should be set depending on Type.
        public string? ValueString { get; init; }
        public decimal? ValueNumber { get; init; }
        public bool? ValueBool { get; init; }
        public string? ValueJson { get; init; }
        public Guid? RefNodeId { get; init; }
        public DateTimeOffset? ValueDate { get; init; }
    }
}
