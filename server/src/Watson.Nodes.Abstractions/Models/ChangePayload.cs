namespace Watson.Nodes
{
    public sealed class ChangePayload
    {
        public string Scope { get; }
        public string CanonicalKey { get; }
        public NodeStatus Status { get; }
        public IReadOnlyList<NodeAttribute>? Attributes { get; }

        public ChangePayload(string scope, string canonicalKey, NodeStatus status, IReadOnlyList<NodeAttribute>? attributes)
        {
            Scope = scope;
            CanonicalKey = canonicalKey;
            Status = status;
            Attributes = attributes;
        }
    }
}
