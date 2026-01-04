namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class AttributeKeyDto
    {
        public required string Key { get; init; }
        public string? Locale { get; init; }
    }
}
