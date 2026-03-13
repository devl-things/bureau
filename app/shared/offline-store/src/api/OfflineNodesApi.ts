import type {
    NodeDto,
    NodeKindDefinition,
    CreateNodeRequest,
    PatchNodeAttributesRequest,
    SearchNodesQuery,
    EdgeDto,
    CreateEdgeRequest,
    RemoveEdgeRequest,
    CursorResponse,
} from "@bureau/nodes-core";
import { NodesApi, NodeStatus } from "@bureau/nodes-core";
import type { IOfflineStorage } from "../storage/IOfflineStorage";
import type { CachedNode, CachedEdge, OfflineNodeDto, OfflineEdgeDto, SyncQueueEntry } from "../storage/types";
import { generateUuid } from "../util/uuid";

function nodeToCached(node: NodeDto): CachedNode {
    const labelAttr = node.attributes.find((a) => a.key === "label");
    return {
        nodeId: node.nodeId,
        kind: node.kind,
        scope: node.scope,
        canonicalKey: node.canonicalKey,
        status: node.status,
        version: node.version,
        dataJson: JSON.stringify(node),
        fetchedAt: Date.now(),
        searchLabel: labelAttr?.valueString ?? "",
    };
}

function cachedToNode(cached: CachedNode): NodeDto {
    return JSON.parse(cached.dataJson) as NodeDto;
}

function edgeToCached(sourceNodeId: string, edge: EdgeDto): CachedEdge {
    return {
        sourceNodeId,
        targetNodeId: edge.targetNodeId,
        purpose: edge.purpose,
        orderIndex: edge.orderIndex,
        dataJson: JSON.stringify(edge),
        fetchedAt: Date.now(),
    };
}

function cachedToEdge(cached: CachedEdge): EdgeDto {
    return JSON.parse(cached.dataJson) as EdgeDto;
}

export class OfflineNodesApi {
    private readonly _remote: NodesApi;
    private readonly _storage: IOfflineStorage;
    private _isOnline: boolean;

    constructor(remote: NodesApi, storage: IOfflineStorage, isOnline: boolean = true) {
        this._remote = remote;
        this._storage = storage;
        this._isOnline = isOnline;
    }

    public setOnline(online: boolean): void {
        this._isOnline = online;
    }

    public get isOnline(): boolean {
        return this._isOnline;
    }

    // ── Search ──────────────────────────────────────────────

    public async searchNodes(
        definition: NodeKindDefinition,
        query: SearchNodesQuery
    ): Promise<CursorResponse<OfflineNodeDto>> {
        if (this._isOnline) {
            try {
                const result = await this._remote.searchNodes(definition, query);

                // Cache results
                const cached = result.data.map(nodeToCached);
                await this._storage.upsertCachedNodes(cached);

                // Merge pending local creates for this kind
                const decorated = await this._decorateNodes(result.data);
                const pendingCreates = await this._getPendingCreatesForKind(definition.kind);
                const all = query.cursor ? decorated : [...pendingCreates, ...decorated];

                return { data: all, meta: result.meta };
            } catch {
                // Fall through to offline path
            }
        }

        // Offline: read from cache
        const cachedNodes = await this._storage.getCachedNodesByKind(
            definition.kind,
            query.query,
            query.limit ?? 25
        );
        const nodes = cachedNodes.map(cachedToNode);
        const decorated = await this._decorateNodes(nodes);

        // Merge pending creates
        const pendingCreates = await this._getPendingCreatesForKind(definition.kind);
        const all = [...pendingCreates, ...decorated];

        return {
            data: all,
            meta: { nextCursor: "", hasMore: false, count: all.length },
        };
    }

    // ── Get Node ────────────────────────────────────────────

    public async getNode(nodeId: string): Promise<OfflineNodeDto> {
        // Check if this is a temp ID — read from queue
        const queueEntries = await this._storage.peekQueue(100);
        const tempEntry = queueEntries.find(
            (e) => e.operation === "createNode" && e.tempNodeId === nodeId
        );
        if (tempEntry) {
            const payload = JSON.parse(tempEntry.payloadJson) as { placeholder: NodeDto };
            const status = tempEntry.status === "failed" ? "failed" as const : "pending" as const;
            return { ...payload.placeholder, _syncStatus: status };
        }

        if (this._isOnline) {
            try {
                const node = await this._remote.getNode(nodeId);
                await this._storage.upsertCachedNode(nodeToCached(node));
                return { ...node, _syncStatus: "synced" };
            } catch {
                // Fall through to offline
            }
        }

        const cached = await this._storage.getCachedNode(nodeId);
        if (cached) {
            return { ...cachedToNode(cached), _syncStatus: "synced" };
        }
        throw new Error(`Node ${nodeId} not found (offline).`);
    }

    // ── Create Node ─────────────────────────────────────────

    public async createNode(
        definition: NodeKindDefinition,
        request: CreateNodeRequest
    ): Promise<OfflineNodeDto> {
        if (this._isOnline) {
            try {
                const node = await this._remote.createNode(definition, request);
                await this._storage.upsertCachedNode(nodeToCached(node));
                return { ...node, _syncStatus: "synced" };
            } catch {
                // Fall through to offline
            }
        }

        // Offline create: generate temp ID, build placeholder, enqueue
        const tempId = `temp-${generateUuid()}`;
        const placeholder: NodeDto = {
            nodeId: tempId,
            kind: definition.kind,
            scope: request.scope ?? "",
            canonicalKey: request.canonicalKey ?? "",
            status: NodeStatus.Active,
            version: 0,
            attributes: request.attributes ?? [],
        };

        const payload = JSON.stringify({
            definitionKind: definition.kind,
            definitionApiRoute: definition.apiRoute,
            request,
            placeholder,
        });

        await this._storage.enqueue("createNode", payload, tempId);
        await this._storage.upsertCachedNode(nodeToCached(placeholder));

        return { ...placeholder, _syncStatus: "pending" };
    }

    // ── Patch Attributes ────────────────────────────────────

    public async patchAttributes(
        nodeId: string,
        patch: PatchNodeAttributesRequest
    ): Promise<void> {
        if (this._isOnline) {
            try {
                await this._remote.patchAttributes(nodeId, patch);
                return;
            } catch {
                // Fall through to offline
            }
        }

        const payload = JSON.stringify({ nodeId, patch });
        await this._storage.enqueue("patchAttributes", payload);
    }

    // ── Edges ───────────────────────────────────────────────

    public async addEdge(nodeId: string, request: CreateEdgeRequest): Promise<void> {
        if (this._isOnline) {
            try {
                await this._remote.addEdge(nodeId, request);
                return;
            } catch {
                // Fall through to offline
            }
        }

        const payload = JSON.stringify({ nodeId, request });
        await this._storage.enqueue("addEdge", payload);
    }

    public async removeEdge(nodeId: string, request: RemoveEdgeRequest): Promise<void> {
        if (this._isOnline) {
            try {
                await this._remote.removeEdge(nodeId, request);
                return;
            } catch {
                // Fall through to offline
            }
        }

        const payload = JSON.stringify({ nodeId, request });
        await this._storage.enqueue("removeEdge", payload);
    }

    public async getEdges(
        nodeId: string,
        purpose?: string,
        cursor?: string,
        limit?: number
    ): Promise<CursorResponse<OfflineEdgeDto>> {
        if (this._isOnline) {
            try {
                const result = await this._remote.getEdges(nodeId, purpose, cursor, limit);

                // Cache edges
                const cached = result.data.map((e) => edgeToCached(nodeId, e));
                await this._storage.upsertCachedEdges(nodeId, cached);

                return result;
            } catch {
                // Fall through
            }
        }

        const cachedEdges = await this._storage.getCachedEdges(nodeId, purpose);
        const edges = cachedEdges.map(cachedToEdge);
        return {
            data: edges,
            meta: { nextCursor: "", hasMore: false, count: edges.length },
        };
    }

    // ── Private helpers ─────────────────────────────────────

    private async _decorateNodes(nodes: NodeDto[]): Promise<OfflineNodeDto[]> {
        const queueEntries = await this._storage.peekQueue(500);
        const pendingNodeIds = new Set<string>();
        const failedNodeIds = new Set<string>();

        for (const entry of queueEntries) {
            if (entry.tempNodeId) {
                if (entry.status === "failed") failedNodeIds.add(entry.tempNodeId);
                else pendingNodeIds.add(entry.tempNodeId);
            }
        }

        return nodes.map((node) => {
            if (failedNodeIds.has(node.nodeId)) return { ...node, _syncStatus: "failed" as const };
            if (pendingNodeIds.has(node.nodeId)) return { ...node, _syncStatus: "pending" as const };
            return { ...node, _syncStatus: "synced" as const };
        });
    }

    private async _getPendingCreatesForKind(kind: number): Promise<OfflineNodeDto[]> {
        const entries = await this._storage.peekQueue(500);
        const creates: OfflineNodeDto[] = [];

        for (const entry of entries) {
            if (entry.operation !== "createNode") continue;
            const payload = JSON.parse(entry.payloadJson) as {
                definitionKind: number;
                placeholder: NodeDto;
            };
            if (payload.definitionKind !== kind) continue;
            const status = entry.status === "failed" ? "failed" as const : "pending" as const;
            creates.push({ ...payload.placeholder, _syncStatus: status });
        }

        return creates;
    }
}
