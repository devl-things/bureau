import type { NodeDto, NodeKind, NodeStatus, EdgeDto } from "@bureau/nodes-core";

export type CachedNode = {
    nodeId: string;
    kind: NodeKind;
    scope: string;
    canonicalKey: string;
    status: NodeStatus;
    version: number;
    dataJson: string; // JSON-serialised NodeDto
    fetchedAt: number; // epoch ms
    searchLabel: string;
};

export type CachedEdge = {
    sourceNodeId: string;
    targetNodeId: string;
    purpose: string;
    orderIndex: number;
    dataJson: string; // JSON-serialised EdgeDto
    fetchedAt: number;
};

export type SyncOperation = "createNode" | "patchAttributes" | "addEdge" | "removeEdge";

export type SyncStatus = "pending" | "in_flight" | "failed";

export type SyncQueueEntry = {
    id: number;
    operation: SyncOperation;
    payloadJson: string;
    tempNodeId: string | null;
    status: SyncStatus;
    errorMessage: string | null;
    createdAt: number;
    retryCount: number;
};

export type OfflineSyncStatus = "synced" | "pending" | "failed";

export type OfflineNodeDto = NodeDto & {
    _syncStatus?: OfflineSyncStatus;
};

export type OfflineEdgeDto = EdgeDto & {
    _syncStatus?: OfflineSyncStatus;
};
