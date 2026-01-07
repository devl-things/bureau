namespace Watson.Nodes.Models
{
    internal class NodeAttributeDb
    {
        public Guid NodeId { get; set; }

        public string Key { get; set; } = string.Empty;

        public string? Locale { get; set; }

        public AttributeValueType Type { get; set; }

        public string? ValueString { get; set; }
        public decimal? ValueNumber { get; set; }
        public bool? ValueBool { get; set; }
        public string? ValueJson { get; set; }
        public Guid? RefNodeId { get; set; }
        public DateTimeOffset? ValueDate { get; set; }

        public NodeDb? Node { get; set; }

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
