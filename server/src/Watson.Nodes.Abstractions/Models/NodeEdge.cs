namespace Watson.Nodes
{
    public sealed class NodeEdge
    {
        public Guid SourceNodeId { get; init; }
        public Guid TargetNodeId { get; init; }
        public string Purpose { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
