using System.Diagnostics.CodeAnalysis;

namespace Watson.Nodes
{
    public sealed class NodeAttribute : NodeAttributeKey
    {
        public AttributeValueType Type { get; init; }

        public string? ValueString { get; init; }
        public decimal? ValueNumber { get; init; }
        public bool? ValueBool { get; init; }
        public string? ValueJson { get; init; }
        public Guid? RefNodeId { get; init; }
        public DateTimeOffset? ValueDate { get; init; }

        [SetsRequiredMembers]
        public NodeAttribute(string key) : base(key)
        {

        }

    }
}
