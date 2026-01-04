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

        public static class Nodes
        {
            public const string Root = "nodes";
            public const string ByIdUrl = $"{Root}/{ByIdSegment}";
            public const string AttributesSegment = $"{ByIdSegment}/attributes";

            public const string AttributesUrl = $"{Root}/{AttributesSegment}";
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
