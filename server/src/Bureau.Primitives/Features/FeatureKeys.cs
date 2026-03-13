namespace Bureau.Primitives.Features;

/// <summary>
/// Canonical feature key constants. Used by APIs ([RequireFeature]) and Sven (scope assignments).
/// TypeScript mirror: app/client/apps/bureau-web/src/features/featureKeys.ts — keep in sync.
/// </summary>
public static class FeatureKeys
{
    public static class Watson
    {
        public const string Root = "watson";

        public static class Nodes
        {
            public const string Root = "watson.nodes";
            public const string Crud = "watson.nodes.crud";
            public const string Analytics = "watson.nodes.analytics";
        }

        public const string Items = "watson.items";
    }

    public static class Niles
    {
        public const string Root = "niles";
        public const string Chores = "niles.chores";
    }

    /// <summary>All defined feature keys (used for dev mode — grant all features).</summary>
    public static IReadOnlyList<string> AllKeys { get; } =
    [
        Watson.Root,
        Watson.Nodes.Root,
        Watson.Nodes.Crud,
        Watson.Nodes.Analytics,
        Watson.Items,
        Niles.Root,
        Niles.Chores,
    ];
}
