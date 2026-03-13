namespace Watson.Nodes
{
    public sealed class CreateEdgeCommand
    {
        public Guid TargetNodeId { get; set; }
        public string? Purpose { get; set; }
        public int OrderIndex { get; set; }
    }
}
