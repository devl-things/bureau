using Bureau;
using Watson.Nodes.Abstractions.Conventions;

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
            base.ValidateCreate(command);
            return CanonicalKeyConventions.RequireCanonicalKey(command.CanonicalKey, Kind.ToString());
        }

        public override List<string> GetDefaultSearchAttributeKeys()
        {
            return DefaultSummaryKeys;
        }

        public override List<string> GetDefaultSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }
    }
}
