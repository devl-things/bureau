using Bureau;

namespace Watson.Nodes.Services
{
    public sealed class ItemNodeKindHandler : DefaultNodeKindHandler
    {
        public override NodeKind Kind
        {
            get { return NodeKind.Item; }
        }

        public override Result ValidateCreate(CreateNodeCommand command)
        {
            return RequireCanonicalKey(command, Kind.ToString());
        }

        public override IReadOnlyList<NodeAttributeKey> GetSearchAttributeKeys()
        {
            return DefaultSummaryKeys;
        }

        public override IReadOnlyList<NodeAttributeKey> GetSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }
    }
}
