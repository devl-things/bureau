using Bureau;

namespace Watson.Nodes.Services
{
    public sealed class TagNodeKindHandler : DefaultNodeKindHandler
    {
        public override NodeKind Kind
        {
            get { return NodeKind.Tag; }
        }

        public override Result ValidateCreate(CreateNodeCommand command)
        {
            return RequireCanonicalKey(command, Kind.ToString());
        }

        public override IReadOnlyList<NodeAttributeKey> GetSearchAttributeKeys()
        {
            return DefaultSearchKeys;
        }

        public override IReadOnlyList<NodeAttributeKey> GetSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }
    }
}
