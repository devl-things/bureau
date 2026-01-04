namespace Watson.Nodes
{
    public sealed class Node
    {
        private readonly List<NodeAttribute> _attributes;

        public Guid NodeId { get; }
        public NodeKind Kind { get; }
        public string Scope { get; }
        public string CanonicalKey { get; private set; }
        public NodeStatus Status { get; private set; }
        public int Version { get; private set; }
        public IReadOnlyCollection<NodeAttribute> Attributes => _attributes;

        public Node(
            Guid nodeId,
            NodeKind kind,
            string scope,
            string canonicalKey,
            NodeStatus status,
            int version,
            IEnumerable<NodeAttribute> attributes)
        {
            NodeId = nodeId;
            Kind = kind;
            Scope = scope;
            CanonicalKey = canonicalKey;
            Status = status;
            Version = version;
            _attributes = new List<NodeAttribute>(attributes);
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
