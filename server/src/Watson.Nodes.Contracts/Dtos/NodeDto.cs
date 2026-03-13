namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class NodeDto
    {
        public required Guid NodeId { get; init; }
        public required NodeKindContract Kind { get; init; }
        public required string Scope { get; init; }
        public required string CanonicalKey { get; init; }
        public required NodeStatusContract Status { get; init; }
        public required int Version { get; init; }
        public required IReadOnlyList<AttributeDto> Attributes { get; init; }
    }
}
