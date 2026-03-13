using Bureau;

namespace Watson.Nodes.Models
{
    internal class NodeEdgeDb : IAuditable
    {
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "watson";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "watson";

        public NodeDb? SourceNode { get; set; }
        public NodeDb? TargetNode { get; set; }
    }
}
