namespace Watson.Nodes
{
    public sealed class Node
    {
        private readonly List<NodeAttribute> _attributes = new List<NodeAttribute>();

        public Guid NodeId { get; init; }
        public NodeKind Kind { get; init; }
        public string Scope { get; init; } = string.Empty;
        public string CanonicalKey { get; private set; }
        public NodeStatus Status { get; private set; }
        public int Version { get; private set; }
        public IReadOnlyCollection<NodeAttribute> Attributes => _attributes;

        public Node(string canonicalKey, int version, NodeStatus status)
        {
            CanonicalKey = canonicalKey;
            Version = version;
            Status = status;
        }
        public void SetCanonicalKey(string canonicalKey)
        {
            CanonicalKey = canonicalKey;
            Version++;
        }

        public void SetAttributes(IEnumerable<NodeAttribute> attributes)
        {
            _attributes.Clear();
            _attributes.AddRange(attributes);
            Version++;
        }

        public void Archive()
        {
            Status = NodeStatus.Archived;
            Version++;
        }
    }
}
