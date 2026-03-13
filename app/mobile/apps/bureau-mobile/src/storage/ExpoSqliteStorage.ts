import type * as SQLite from "expo-sqlite";
import type { NodeKind } from "@bureau/nodes-core";
import type {
    IOfflineStorage,
    CachedNode,
    CachedEdge,
    SyncQueueEntry,
    SyncOperation,
    SyncStatus,
} from "@bureau/offline-store";
import { migrations } from "./migrations";

export class ExpoSqliteStorage implements IOfflineStorage {
    private readonly _db: SQLite.SQLiteDatabase;

    constructor(db: SQLite.SQLiteDatabase) {
        this._db = db;
    }

    // ── Lifecycle ───────────────────────────────────────────

    async initialize(): Promise<void> {
        const currentVersion = await this._getSchemaVersion();
        for (const migration of migrations) {
            if (migration.version <= currentVersion) continue;
            for (const sql of migration.statements) {
                await this._db.runAsync(sql);
            }
            await this._db.runAsync(
                `INSERT OR REPLACE INTO meta (key, value) VALUES ('schema_version', ?)`,
                [String(migration.version)]
            );
        }
    }

    private async _getSchemaVersion(): Promise<number> {
        try {
            // meta table might not exist yet
            const row = await this._db.getFirstAsync<{ value: string }>(
                `SELECT value FROM meta WHERE key = 'schema_version'`
            );
            return row ? parseInt(row.value, 10) : 0;
        } catch {
            return 0;
        }
    }

    // ── Node cache ──────────────────────────────────────────

    async getCachedNode(nodeId: string): Promise<CachedNode | null> {
        const row = await this._db.getFirstAsync<CachedNodeRow>(
            `SELECT * FROM cached_nodes WHERE node_id = ?`,
            [nodeId]
        );
        return row ? rowToNode(row) : null;
    }

    async getCachedNodesByKind(kind: NodeKind, query?: string, limit?: number): Promise<CachedNode[]> {
        let sql = `SELECT * FROM cached_nodes WHERE kind = ?`;
        const params: (string | number)[] = [kind];

        if (query) {
            sql += ` AND search_label LIKE ?`;
            params.push(`%${query}%`);
        }
        sql += ` ORDER BY fetched_at DESC`;
        if (limit) {
            sql += ` LIMIT ?`;
            params.push(limit);
        }

        const rows = await this._db.getAllAsync<CachedNodeRow>(sql, params);
        return rows.map(rowToNode);
    }

    async upsertCachedNode(node: CachedNode): Promise<void> {
        await this._db.runAsync(
            `INSERT OR REPLACE INTO cached_nodes
                (node_id, kind, scope, canonical_key, status, version, data_json, fetched_at, search_label)
             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`,
            [
                node.nodeId, node.kind, node.scope, node.canonicalKey,
                node.status, node.version, node.dataJson, node.fetchedAt, node.searchLabel,
            ]
        );
    }

    async upsertCachedNodes(nodes: CachedNode[]): Promise<void> {
        for (const node of nodes) {
            await this.upsertCachedNode(node);
        }
    }

    async deleteCachedNode(nodeId: string): Promise<void> {
        await this._db.runAsync(`DELETE FROM cached_nodes WHERE node_id = ?`, [nodeId]);
    }

    // ── Edge cache ──────────────────────────────────────────

    async getCachedEdges(sourceNodeId: string, purpose?: string): Promise<CachedEdge[]> {
        let sql = `SELECT * FROM cached_edges WHERE source_node_id = ?`;
        const params: string[] = [sourceNodeId];
        if (purpose) {
            sql += ` AND purpose = ?`;
            params.push(purpose);
        }
        sql += ` ORDER BY order_index`;
        const rows = await this._db.getAllAsync<CachedEdgeRow>(sql, params);
        return rows.map(rowToEdge);
    }

    async upsertCachedEdges(sourceNodeId: string, edges: CachedEdge[]): Promise<void> {
        // Replace all edges for this source
        await this._db.runAsync(`DELETE FROM cached_edges WHERE source_node_id = ?`, [sourceNodeId]);
        for (const edge of edges) {
            await this._db.runAsync(
                `INSERT INTO cached_edges
                    (source_node_id, target_node_id, purpose, order_index, data_json, fetched_at)
                 VALUES (?, ?, ?, ?, ?, ?)`,
                [
                    edge.sourceNodeId, edge.targetNodeId, edge.purpose,
                    edge.orderIndex, edge.dataJson, edge.fetchedAt,
                ]
            );
        }
    }

    async deleteCachedEdge(sourceNodeId: string, targetNodeId: string, purpose: string): Promise<void> {
        await this._db.runAsync(
            `DELETE FROM cached_edges WHERE source_node_id = ? AND target_node_id = ? AND purpose = ?`,
            [sourceNodeId, targetNodeId, purpose]
        );
    }

    // ── Sync queue ──────────────────────────────────────────

    async enqueue(operation: SyncOperation, payloadJson: string, tempNodeId?: string): Promise<number> {
        const result = await this._db.runAsync(
            `INSERT INTO sync_queue (operation, payload_json, temp_node_id, status, created_at, retry_count)
             VALUES (?, ?, ?, 'pending', ?, 0)`,
            [operation, payloadJson, tempNodeId ?? null, Date.now()]
        );
        return result.lastInsertRowId;
    }

    async peekQueue(limit: number = 10): Promise<SyncQueueEntry[]> {
        const rows = await this._db.getAllAsync<SyncQueueRow>(
            `SELECT * FROM sync_queue WHERE status IN ('pending', 'failed') ORDER BY id LIMIT ?`,
            [limit]
        );
        return rows.map(rowToQueueEntry);
    }

    async markInFlight(id: number): Promise<void> {
        await this._db.runAsync(
            `UPDATE sync_queue SET status = 'in_flight' WHERE id = ?`,
            [id]
        );
    }

    async markSynced(id: number): Promise<void> {
        await this._db.runAsync(
            `UPDATE sync_queue SET status = 'synced' WHERE id = ?`,
            [id]
        );
    }

    async markFailed(id: number, errorMessage: string): Promise<void> {
        await this._db.runAsync(
            `UPDATE sync_queue SET status = 'failed', error_message = ?, retry_count = retry_count + 1 WHERE id = ?`,
            [errorMessage, id]
        );
    }

    async removeFromQueue(id: number): Promise<void> {
        await this._db.runAsync(`DELETE FROM sync_queue WHERE id = ?`, [id]);
    }

    async getQueueCount(): Promise<number> {
        const row = await this._db.getFirstAsync<{ cnt: number }>(
            `SELECT COUNT(*) as cnt FROM sync_queue WHERE status IN ('pending', 'failed')`
        );
        return row?.cnt ?? 0;
    }

    async replaceTempNodeId(oldId: string, newId: string): Promise<void> {
        // Update temp_node_id references in queue
        await this._db.runAsync(
            `UPDATE sync_queue SET temp_node_id = ? WHERE temp_node_id = ?`,
            [newId, oldId]
        );
        // Also replace in payload JSON (for edge operations referencing the temp node)
        await this._db.runAsync(
            `UPDATE sync_queue SET payload_json = REPLACE(payload_json, ?, ?) WHERE payload_json LIKE ?`,
            [oldId, newId, `%${oldId}%`]
        );
    }

    // ── Meta ────────────────────────────────────────────────

    async getMeta(key: string): Promise<string | null> {
        const row = await this._db.getFirstAsync<{ value: string }>(
            `SELECT value FROM meta WHERE key = ?`,
            [key]
        );
        return row?.value ?? null;
    }

    async setMeta(key: string, value: string): Promise<void> {
        await this._db.runAsync(
            `INSERT OR REPLACE INTO meta (key, value) VALUES (?, ?)`,
            [key, value]
        );
    }
}

// ── Row types & mappers ─────────────────────────────────

type CachedNodeRow = {
    node_id: string;
    kind: number;
    scope: string;
    canonical_key: string;
    status: number;
    version: number;
    data_json: string;
    fetched_at: number;
    search_label: string;
};

function rowToNode(row: CachedNodeRow): CachedNode {
    return {
        nodeId: row.node_id,
        kind: row.kind,
        scope: row.scope,
        canonicalKey: row.canonical_key,
        status: row.status,
        version: row.version,
        dataJson: row.data_json,
        fetchedAt: row.fetched_at,
        searchLabel: row.search_label,
    };
}

type CachedEdgeRow = {
    source_node_id: string;
    target_node_id: string;
    purpose: string;
    order_index: number;
    data_json: string;
    fetched_at: number;
};

function rowToEdge(row: CachedEdgeRow): CachedEdge {
    return {
        sourceNodeId: row.source_node_id,
        targetNodeId: row.target_node_id,
        purpose: row.purpose,
        orderIndex: row.order_index,
        dataJson: row.data_json,
        fetchedAt: row.fetched_at,
    };
}

type SyncQueueRow = {
    id: number;
    operation: string;
    payload_json: string;
    temp_node_id: string | null;
    status: string;
    error_message: string | null;
    created_at: number;
    retry_count: number;
};

function rowToQueueEntry(row: SyncQueueRow): SyncQueueEntry {
    return {
        id: row.id,
        operation: row.operation as SyncOperation,
        payloadJson: row.payload_json,
        tempNodeId: row.temp_node_id,
        status: row.status as SyncStatus,
        errorMessage: row.error_message,
        createdAt: row.created_at,
        retryCount: row.retry_count,
    };
}
