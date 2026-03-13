import React, { useMemo } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "@bureau/bureau-shell";
import type { NavItem } from "@bureau/bureau-shell";
import { FeatureBoundary } from "./features/FeatureBoundary";
import { FeatureUnavailable } from "./features/FeatureUnavailable";
import { FeatureKeys } from "./features/featureKeys";
import { useFeature } from "./features/useFeature";

const ChoresAdmin = React.lazy(() =>
    import("@bureau/chores-admin").then((m) => ({ default: m.ChoresAdminApp }))
);
const NodesAdmin = React.lazy(() =>
    import("@bureau/nodes-admin").then((m) => ({ default: m.NodesAdminApp }))
);

export function App(): React.ReactElement {
    const choresEnabled = useFeature(FeatureKeys.Niles.Chores);
    const nodesEnabled = useFeature(FeatureKeys.Watson.Nodes.Root);

    const navItems = useMemo<NavItem[]>(() => {
        const items: NavItem[] = [];
        if (nodesEnabled) items.push({ key: "nodes", title: "Nodes", path: "/nodes" });
        if (choresEnabled) items.push({ key: "chores", title: "Chores", path: "/chores" });
        return items;
    }, [choresEnabled, nodesEnabled]);

    const defaultPath = navItems[0]?.path ?? "/nodes";

    return (
        <Routes>
            <Route element={<Layout navItems={navItems} />}>
                <Route index element={<Navigate to={defaultPath} replace />} />
                {nodesEnabled && (
                    <Route
                        path="nodes/*"
                        element={
                            <FeatureBoundary
                                feature={FeatureKeys.Watson.Nodes.Root}
                                fallback={<FeatureUnavailable name="Nodes" />}
                            >
                                <NodesAdmin />
                            </FeatureBoundary>
                        }
                    />
                )}
                {choresEnabled && (
                    <Route
                        path="chores/*"
                        element={
                            <FeatureBoundary
                                feature={FeatureKeys.Niles.Chores}
                                fallback={<FeatureUnavailable name="Chores" />}
                            >
                                <ChoresAdmin />
                            </FeatureBoundary>
                        }
                    />
                )}
            </Route>
        </Routes>
    );
}
