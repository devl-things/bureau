namespace Niles.Chores.Contracts
{
    public static class ApiRoutes
    {
        public const string ByIdSegment = "{id}";
        public static class Chores
        {
            public const string Root = "chores";
            public const string CriticalSegment = "{id}/critical";

            public const string ByIdUrl = $"{Root}/{ByIdSegment}";
            public const string CriticalUrl = $"{Root}/{CriticalSegment}";
        }

        public static class Housekeeping
        {
            public const string Root = "housekeeping";
            public const string PrioritizedChoresSegment = "prioritized-chores";
            public const string SubmitSegment = "submit";

            public const string ByIdUrl = $"{Root}/{ByIdSegment}";
            public const string PrioritizedChoresUrl = $"{Root}/{PrioritizedChoresSegment}";
            public const string SubmitUrl = $"{Root}/{SubmitSegment}";
        }

        public static class Health
        {
            public const string Root = "health";
        }

        public static class Seed
        {
            public const string Root = "seed";
            public const string TestSegment = "test";
            public const string TestClearSegment = "test/clear";

        }
    }
}
