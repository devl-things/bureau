import { useCallback, useEffect, useRef, useState } from "react";
import type { SyncService, SyncResult } from "../sync/SyncService";

const SYNC_INTERVAL_MS = 30_000;

export type UseSyncReturn = {
    syncNow: () => Promise<void>;
    pendingCount: number;
    lastResult: SyncResult | null;
    syncing: boolean;
};

export function useSync(
    syncService: SyncService,
    isOnline: boolean,
    getQueueCount: () => Promise<number>
): UseSyncReturn {
    const [pendingCount, setPendingCount] = useState(0);
    const [lastResult, setLastResult] = useState<SyncResult | null>(null);
    const [syncing, setSyncing] = useState(false);
    const prevOnlineRef = useRef(isOnline);

    const refreshCount = useCallback(async () => {
        const count = await getQueueCount();
        setPendingCount(count);
    }, [getQueueCount]);

    const syncNow = useCallback(async () => {
        if (syncing) return;
        setSyncing(true);
        try {
            const result = await syncService.sync();
            setLastResult(result);
            await refreshCount();
        } finally {
            setSyncing(false);
        }
    }, [syncService, syncing, refreshCount]);

    // Sync on online transition
    useEffect(() => {
        if (isOnline && !prevOnlineRef.current) {
            void syncNow();
        }
        prevOnlineRef.current = isOnline;
    }, [isOnline, syncNow]);

    // Periodic sync while online
    useEffect(() => {
        if (!isOnline) return;

        const interval = setInterval(() => {
            void syncNow();
        }, SYNC_INTERVAL_MS);

        return () => clearInterval(interval);
    }, [isOnline, syncNow]);

    // Initial count
    useEffect(() => {
        void refreshCount();
    }, [refreshCount]);

    return { syncNow, pendingCount, lastResult, syncing };
}
