namespace Watson.Nodes
{
    public sealed class RemoveEdgeCommand
    {
        public Guid TargetNodeId { get; set; }
        public string? Purpose { get; set; }
    }
}
