namespace Watson.Nodes
{
    public sealed class Node
    {
        private readonly List<NodeAttribute> _attributes = new List<NodeAttribute>();

        public Guid NodeId { get; init; }
        public NodeKind Kind { get; init; }
        public string Scope { get; init; } = string.Empty;
        public string CanonicalKey { get; private set; } = string.Empty;
        public NodeStatus Status { get; private set; }
        public int Version { get; private set; }
        public IReadOnlyCollection<NodeAttribute> Attributes => _attributes;

        public Node()
        {
            Version = 1;
            Status = NodeStatus.Active;
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
