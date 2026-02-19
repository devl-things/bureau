using Bureau;
using Watson.Nodes.Abstractions.Conventions;

namespace Watson.Nodes.Models
{
    internal class NodeAttributeDb : IAuditable
    {
        public Guid NodeId { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Locale { get; set; } = LocaleConventions.DefaultLocale;

        public AttributeValueType Type { get; set; }

        public string? ValueString { get; set; }
        public decimal? ValueNumber { get; set; }
        public bool? ValueBool { get; set; }
        public string? ValueJson { get; set; }
        public Guid? RefNodeId { get; set; }
        public DateTimeOffset? ValueDate { get; set; }

        public int OrderIndex { get; set; }
        public NodeDb? Node { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "watson";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "watson";

        public void ApplyFromDomain(NodeAttribute attribute)
        {
            Type = attribute.Type;
            ValueString = attribute.ValueString;
            ValueNumber = attribute.ValueNumber;
            ValueBool = attribute.ValueBool;
            ValueJson = attribute.ValueJson;
            RefNodeId = attribute.RefNodeId;
            ValueDate = attribute.ValueDate;
        }
    }
}
