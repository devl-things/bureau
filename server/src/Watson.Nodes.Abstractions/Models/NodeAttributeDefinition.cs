namespace Watson.Nodes
{
    public class NodeAttributeDefinition
    {
        public required string Key { get; init; }
        public AttributeValueType Type { get; init; }
        public bool IsRequired { get; init; }
        public int OrderIndex { get; init; }
    }
}
