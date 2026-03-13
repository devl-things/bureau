import React, { useCallback, useEffect, useState } from "react";
import type { NodeDto, NodeKindDefinition, NodesApi, CursorResponse } from "@bureau/nodes-core";
import { NodeTable } from "../components/NodeTable";
import { NodeCreatePage } from "./NodeCreatePage";
import { NodeDetailPage } from "./NodeDetailPage";

type Props = {
    definition: NodeKindDefinition;
    nodesApi: NodesApi;
};

type View = { type: "list" } | { type: "create" } | { type: "detail"; node: NodeDto };

const PAGE_SIZE = 25;

export function NodeListPage({ definition, nodesApi }: Props): React.ReactElement {
    const [nodes, setNodes] = useState<NodeDto[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(false);
    const [loading, setLoading] = useState(false);
    const [search, setSearch] = useState("");
    const [view, setView] = useState<View>({ type: "list" });

    const loadNodes = useCallback(
        async (cursor?: string) => {
            setLoading(true);
            try {
                const result: CursorResponse<NodeDto> = await nodesApi.searchNodes(definition, {
                    query: search.trim() || undefined,
                    cursor,
                    limit: PAGE_SIZE,
                });
                if (cursor) {
                    setNodes((prev) => [...prev, ...result.data]);
                } else {
                    setNodes(result.data);
                }
                setNextCursor(result.meta.nextCursor ?? null);
                setHasMore(result.meta.hasMore);
            } catch {
                if (!cursor) setNodes([]);
            } finally {
                setLoading(false);
            }
        },
        [definition, nodesApi, search]
    );

    useEffect(() => {
        void loadNodes();
    }, [loadNodes]);

    function handleLoadMore(): void {
        if (nextCursor) {
            void loadNodes(nextCursor);
        }
    }

    function handleCreated(): void {
        setView({ type: "list" });
        void loadNodes();
    }

    function handleBack(): void {
        setView({ type: "list" });
        void loadNodes();
    }

    if (view.type === "create") {
        return (
            <NodeCreatePage
                definition={definition}
                nodesApi={nodesApi}
                onCreated={handleCreated}
                onCancel={() => setView({ type: "list" })}
            />
        );
    }

    if (view.type === "detail") {
        return (
            <NodeDetailPage
                node={view.node}
                definition={definition}
                nodesApi={nodesApi}
                onBack={handleBack}
            />
        );
    }

    return (
        <div>
            <div className="controls">
                <input
                    type="search"
                    placeholder={`Search ${definition.pluralLabel.toLowerCase()}...`}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                />
                <button type="button" onClick={() => setView({ type: "create" })}>
                    + Create {definition.label}
                </button>
            </div>

            <div style={{ marginTop: 14 }}>
                <NodeTable
                    definition={definition}
                    nodes={nodes}
                    onSelect={(node) => setView({ type: "detail", node })}
                    hasMore={hasMore}
                    onLoadMore={handleLoadMore}
                    loading={loading}
                />
            </div>
        </div>
    );
}
