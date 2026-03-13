namespace Watson.Nodes
{
    public sealed class PatchNodeAttributesCommand
    {
        public IEnumerable<NodeAttribute>? Set { get; init; }
        public IEnumerable<NodeAttributeKey>? Remove { get; init; }
    }
}
