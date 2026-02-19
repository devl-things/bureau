namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class CreateEdgeRequest
    {
        public Guid TargetNodeId { get; init; }
        public string? Purpose { get; init; }
        public int OrderIndex { get; init; }
    }
}
