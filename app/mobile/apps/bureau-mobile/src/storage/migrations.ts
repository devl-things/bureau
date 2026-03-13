export type Migration = {
    version: number;
    statements: string[];
};

export const migrations: Migration[] = [
    {
        version: 1,
        statements: [
            `CREATE TABLE IF NOT EXISTS cached_nodes (
                node_id TEXT PRIMARY KEY,
                kind INTEGER NOT NULL,
                scope TEXT DEFAULT '',
                canonical_key TEXT DEFAULT '',
                status INTEGER DEFAULT 1,
                version INTEGER DEFAULT 0,
                data_json TEXT NOT NULL,
                fetched_at INTEGER NOT NULL,
                search_label TEXT
            )`,
            `CREATE INDEX IF NOT EXISTS idx_cached_nodes_kind ON cached_nodes(kind)`,
            `CREATE INDEX IF NOT EXISTS idx_cached_nodes_label ON cached_nodes(search_label)`,

            `CREATE TABLE IF NOT EXISTS cached_edges (
                source_node_id TEXT NOT NULL,
                target_node_id TEXT NOT NULL,
                purpose TEXT NOT NULL,
                order_index INTEGER DEFAULT 0,
                data_json TEXT NOT NULL,
                fetched_at INTEGER NOT NULL,
                PRIMARY KEY (source_node_id, target_node_id, purpose)
            )`,

            `CREATE TABLE IF NOT EXISTS sync_queue (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                operation TEXT NOT NULL,
                payload_json TEXT NOT NULL,
                temp_node_id TEXT,
                status TEXT DEFAULT 'pending',
                error_message TEXT,
                created_at INTEGER NOT NULL,
                retry_count INTEGER DEFAULT 0
            )`,

            `CREATE TABLE IF NOT EXISTS meta (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            )`,
        ],
    },
];
