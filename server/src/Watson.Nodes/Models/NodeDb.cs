namespace Watson.Nodes.Models
{
    internal class NodeDb
    {
        public Guid NodeId { get; set; }

        public long CreatedSequence { get; set; }

        public NodeKind Kind { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string? CanonicalKey { get; set; }

        public NodeStatus Status { get; set; }

        public int Version { get; set; }
    }
}
