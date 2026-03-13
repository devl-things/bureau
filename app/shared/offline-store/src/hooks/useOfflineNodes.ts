import { useCallback, useEffect, useState } from "react";
import type { NodeKindDefinition, SearchNodesQuery, CursorResponse } from "@bureau/nodes-core";
import type { OfflineNodeDto } from "../storage/types";
import type { OfflineNodesApi } from "../api/OfflineNodesApi";

const PAGE_SIZE = 25;

export type UseOfflineNodesReturn = {
    nodes: OfflineNodeDto[];
    loading: boolean;
    hasMore: boolean;
    loadMore: () => void;
    refresh: () => void;
};

export function useOfflineNodes(
    offlineApi: OfflineNodesApi,
    definition: NodeKindDefinition,
    searchQuery: string
): UseOfflineNodesReturn {
    const [nodes, setNodes] = useState<OfflineNodeDto[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(false);
    const [loading, setLoading] = useState(false);

    const load = useCallback(
        async (cursor?: string) => {
            setLoading(true);
            try {
                const query: SearchNodesQuery = {
                    query: searchQuery.trim() || undefined,
                    cursor: cursor ?? undefined,
                    limit: PAGE_SIZE,
                };
                const result: CursorResponse<OfflineNodeDto> = await offlineApi.searchNodes(
                    definition,
                    query
                );
                if (cursor) {
                    setNodes((prev) => [...prev, ...result.data]);
                } else {
                    setNodes(result.data);
                }
                setNextCursor(result.meta.nextCursor || null);
                setHasMore(result.meta.hasMore);
            } catch {
                if (!cursor) setNodes([]);
            } finally {
                setLoading(false);
            }
        },
        [offlineApi, definition, searchQuery]
    );

    useEffect(() => {
        void load();
    }, [load]);

    const loadMore = useCallback(() => {
        if (hasMore && nextCursor) void load(nextCursor);
    }, [hasMore, nextCursor, load]);

    const refresh = useCallback(() => {
        void load();
    }, [load]);

    return { nodes, loading, hasMore, loadMore, refresh };
}
