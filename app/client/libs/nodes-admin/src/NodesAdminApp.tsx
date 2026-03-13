import React, { useMemo, useState } from "react";
import { AppRuntimeOptionsLoader } from "@bureau/client-core";
import {
    ItemDefinition,
    TagDefinition,
    VariantDefinition,
    ProjectDefinition,
    type NodeKindDefinition,
} from "@bureau/nodes-core";
import { createAdminApi } from "./api/createAdminClient";
import { NodeListPage } from "./pages/NodeListPage";
import "./styles/nodes-admin.css";

type TabKey = "items" | "tags" | "variants" | "projects";

const tabs: { key: TabKey; definition: NodeKindDefinition }[] = [
    { key: "items", definition: ItemDefinition },
    { key: "tags", definition: TagDefinition },
    { key: "variants", definition: VariantDefinition },
    { key: "projects", definition: ProjectDefinition },
];

export function NodesAdminApp(): React.ReactElement {
    const { nodesApi, runtime } = useMemo(() => createAdminApi(), []);
    const [activeTab, setActiveTab] = useState<TabKey>("items");

    const headerRuntime = useMemo(() => {
        return runtime ?? AppRuntimeOptionsLoader.loadFromWindow();
    }, [runtime]);

    const currentTab = tabs.find((t) => t.key === activeTab)!;

    return (
        <div className="shell">
            <header>
                <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
                    <div>
                        <div className="title">Nodes admin</div>
                        <div className="subtitle">Manage items, tags, variants and projects.</div>
                    </div>

                    <div style={{ color: "var(--muted)", fontSize: "0.9rem", alignSelf: "flex-end" }}>
                        env: {headerRuntime.environment} &bull; version: {headerRuntime.version}
                    </div>
                </div>

                <div className="breadcrumbs" style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                    {tabs.map((tab) => (
                        <button
                            key={tab.key}
                            type="button"
                            className="btn ghost"
                            style={{
                                borderColor: activeTab === tab.key ? "rgba(56, 189, 248, 0.55)" : undefined,
                            }}
                            onClick={() => setActiveTab(tab.key)}
                        >
                            {tab.definition.pluralLabel}
                        </button>
                    ))}
                </div>
            </header>

            <NodeListPage
                key={currentTab.key}
                definition={currentTab.definition}
                nodesApi={nodesApi}
            />
        </div>
    );
}
