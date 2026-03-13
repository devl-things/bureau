import type { NodeKind } from "@bureau/nodes-core";
import type { CachedNode, CachedEdge, SyncQueueEntry, SyncOperation } from "./types";

export interface IOfflineStorage {
    // Lifecycle
    initialize(): Promise<void>;

    // Node cache
    getCachedNode(nodeId: string): Promise<CachedNode | null>;
    getCachedNodesByKind(kind: NodeKind, query?: string, limit?: number): Promise<CachedNode[]>;
    upsertCachedNode(node: CachedNode): Promise<void>;
    upsertCachedNodes(nodes: CachedNode[]): Promise<void>;
    deleteCachedNode(nodeId: string): Promise<void>;

    // Edge cache
    getCachedEdges(sourceNodeId: string, purpose?: string): Promise<CachedEdge[]>;
    upsertCachedEdges(sourceNodeId: string, edges: CachedEdge[]): Promise<void>;
    deleteCachedEdge(sourceNodeId: string, targetNodeId: string, purpose: string): Promise<void>;

    // Sync queue
    enqueue(operation: SyncOperation, payloadJson: string, tempNodeId?: string): Promise<number>;
    peekQueue(limit?: number): Promise<SyncQueueEntry[]>;
    markInFlight(id: number): Promise<void>;
    markSynced(id: number): Promise<void>;
    markFailed(id: number, errorMessage: string): Promise<void>;
    removeFromQueue(id: number): Promise<void>;
    getQueueCount(): Promise<number>;
    replaceTempNodeId(oldId: string, newId: string): Promise<void>;

    // Metadata
    getMeta(key: string): Promise<string | null>;
    setMeta(key: string, value: string): Promise<void>;
}
