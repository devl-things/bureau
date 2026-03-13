import type { NodesApi, NodeKindDefinition, CreateNodeRequest, PatchNodeAttributesRequest, CreateEdgeRequest, RemoveEdgeRequest } from "@bureau/nodes-core";
import { AllDefinitions } from "@bureau/nodes-core";
import type { IOfflineStorage } from "../storage/IOfflineStorage";
import type { SyncQueueEntry } from "../storage/types";

function nodeToCached(node: import("@bureau/nodes-core").NodeDto) {
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

export type SyncResult = {
    processed: number;
    failed: boolean;
    error?: string;
};

export class SyncService {
    private readonly _remote: NodesApi;
    private readonly _storage: IOfflineStorage;
    private _running = false;

    constructor(remote: NodesApi, storage: IOfflineStorage) {
        this._remote = remote;
        this._storage = storage;
    }

    public get isRunning(): boolean {
        return this._running;
    }

    public async sync(): Promise<SyncResult> {
        if (this._running) return { processed: 0, failed: false };
        this._running = true;

        let processed = 0;
        try {
            while (true) {
                const entries = await this._storage.peekQueue(1);
                if (entries.length === 0) break;

                const entry = entries[0];
                await this._storage.markInFlight(entry.id);

                try {
                    await this._processEntry(entry);
                    await this._storage.markSynced(entry.id);
                    await this._storage.removeFromQueue(entry.id);
                    processed++;
                } catch (err) {
                    const message = err instanceof Error ? err.message : "Unknown error";
                    await this._storage.markFailed(entry.id, message);
                    return { processed, failed: true, error: message };
                }
            }
            return { processed, failed: false };
        } finally {
            this._running = false;
        }
    }

    private async _processEntry(entry: SyncQueueEntry): Promise<void> {
        switch (entry.operation) {
            case "createNode":
                await this._syncCreate(entry);
                break;
            case "patchAttributes":
                await this._syncPatch(entry);
                break;
            case "addEdge":
                await this._syncAddEdge(entry);
                break;
            case "removeEdge":
                await this._syncRemoveEdge(entry);
                break;
        }
    }

    private async _syncCreate(entry: SyncQueueEntry): Promise<void> {
        const payload = JSON.parse(entry.payloadJson) as {
            definitionKind: number;
            definitionApiRoute: string;
            request: CreateNodeRequest;
            placeholder: import("@bureau/nodes-core").NodeDto;
        };

        const definition = AllDefinitions.find((d) => d.kind === payload.definitionKind);
        if (!definition) throw new Error(`No definition for kind ${payload.definitionKind}`);

        const realNode = await this._remote.createNode(definition, payload.request);

        // Replace temp ID with real ID in cache and remaining queue entries
        if (entry.tempNodeId) {
            await this._storage.deleteCachedNode(entry.tempNodeId);
            await this._storage.upsertCachedNode(nodeToCached(realNode));
            await this._storage.replaceTempNodeId(entry.tempNodeId, realNode.nodeId);
        }
    }

    private async _syncPatch(entry: SyncQueueEntry): Promise<void> {
        const payload = JSON.parse(entry.payloadJson) as {
            nodeId: string;
            patch: PatchNodeAttributesRequest;
        };
        await this._remote.patchAttributes(payload.nodeId, payload.patch);
    }

    private async _syncAddEdge(entry: SyncQueueEntry): Promise<void> {
        const payload = JSON.parse(entry.payloadJson) as {
            nodeId: string;
            request: CreateEdgeRequest;
        };
        await this._remote.addEdge(payload.nodeId, payload.request);
    }

    private async _syncRemoveEdge(entry: SyncQueueEntry): Promise<void> {
        const payload = JSON.parse(entry.payloadJson) as {
            nodeId: string;
            request: RemoveEdgeRequest;
        };
        await this._remote.removeEdge(payload.nodeId, payload.request);
    }
}
