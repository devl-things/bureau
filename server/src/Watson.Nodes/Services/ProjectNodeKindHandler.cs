namespace Watson.Nodes.Services
{
    public sealed class ProjectNodeKindHandler : DefaultNodeKindHandler
    {
        public override NodeKind Kind
        {
            get { return NodeKind.Project; }
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
