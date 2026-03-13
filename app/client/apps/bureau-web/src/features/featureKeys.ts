// Mirror of Bureau.Features.FeatureKeys (C#). Keep in sync manually.
// When it becomes friction, generate this file from the C# constants at build time.
export const FeatureKeys = {
    Watson: {
        Root: "watson",
        Nodes: {
            Root: "watson.nodes",
            Crud: "watson.nodes.crud",
            Analytics: "watson.nodes.analytics",
        },
        Items: "watson.items",
    },
    Niles: {
        Root: "niles",
        Chores: "niles.chores",
    },
} as const;
