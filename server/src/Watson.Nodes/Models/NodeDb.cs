using Bureau;

namespace Watson.Nodes.Models
{
    internal class NodeDb : IAuditable
    {
        public Guid NodeId { get; set; }

        public long CreatedSequence { get; set; }

        public NodeKind Kind { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string? CanonicalKey { get; set; }

        public NodeStatus Status { get; set; }

        public int Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "watson";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "watson";

        public ICollection<NodeAttributeDb> NodeAttributes { get; set; } = [];
    }
}
