namespace Watson.Nodes
{
    public sealed class CreateNodeCommand
    {
        public NodeKind Kind { get; set; }
        public string? Scope { get; set; }
        public string? CanonicalKey { get; set; }
        public IReadOnlyList<NodeAttribute> Attributes { get; set; }
    }
}
