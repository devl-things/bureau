namespace Watson.Nodes.Contracts
{
    public static class ApiRoutes
    {
        public const string ByIdSegment = "{nodeId:guid}";

        public static class Items
        {
            public const string Root = "items";
        }

        public static class Tags
        {
            public const string Root = "tags";
        }

        public static class Variants
        {
            public const string Root = "variants";
        }

        public static class Projects
        {
            public const string Root = "projects";
        }

        public static class Nodes
        {
            public const string Root = "nodes";
            public const string ByIdRouteName = "Nodes.GetById";
            public const string ByIdUrl = $"{Root}/{ByIdSegment}";
            public const string AttributesSegment = $"{ByIdSegment}/attributes";
            public const string AttributesUrl = $"{Root}/{AttributesSegment}";
            public const string EdgesSegment = $"{ByIdSegment}/edges";
            public const string EdgesUrl = $"{Root}/{EdgesSegment}";
        }

        public static class Changes
        {
            public const string Root = "changes";
        }

        public static class Health
        {
            public const string Root = "health";
        }
    }
}
