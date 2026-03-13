import { createContext, useContext } from "react";
import type { OfflineNodesApi } from "../api/OfflineNodesApi";
import type { SyncService } from "../sync/SyncService";
import type { IOfflineStorage } from "../storage/IOfflineStorage";

export type OfflineContextValue = {
    offlineApi: OfflineNodesApi;
    syncService: SyncService;
    storage: IOfflineStorage;
    isOnline: boolean;
};

export const OfflineContext = createContext<OfflineContextValue | null>(null);

export function useOfflineContext(): OfflineContextValue {
    const ctx = useContext(OfflineContext);
    if (!ctx) {
        throw new Error("useOfflineContext must be used within an OfflineContext.Provider");
    }
    return ctx;
}
