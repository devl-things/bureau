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
            base.ValidateCreate(command);
            return RequireCanonicalKey(command, Kind.ToString());
        }

        public override List<string> GetDefaultSearchAttributeKeys()
        {
            return DefaultSearchKeys;
        }

        public override List<string> GetDefaultSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }
    }
}
