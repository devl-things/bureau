namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class EdgeDto
    {
        public required Guid TargetNodeId { get; init; }
        public required string Purpose { get; init; }
        public required int OrderIndex { get; init; }
    }
}
