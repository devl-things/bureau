namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class RemoveEdgeRequest
    {
        public Guid TargetNodeId { get; init; }
        public string? Purpose { get; init; }
    }
}
