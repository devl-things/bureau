namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class ChangePayloadDto
    {
        public required string Scope { get; init; }
        public required string CanonicalKey { get; init; }
        public required NodeStatusContract Status { get; init; }
        public IReadOnlyList<AttributeDto>? Attributes { get; init; }
    }
}
