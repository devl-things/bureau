import React, { useCallback, useEffect, useMemo, useState } from "react";
import { ActivityIndicator, View } from "react-native";
import { NavigationContainer } from "@react-navigation/native";
import { ApiClient } from "@bureau/client-core";
import { NodesApi } from "@bureau/nodes-core";
import {
    OfflineNodesApi,
    SyncService,
    OfflineContext,
    useConnectivity,
    useSync,
} from "@bureau/offline-store";
import type { IOfflineStorage } from "@bureau/offline-store";
import { loadAppConfig } from "./src/config/appConfig";
import { getStorage } from "./src/storage/storageProvider";
import { netInfoConnectivity } from "./src/storage/netInfoConnectivity";
import { AppNavigator } from "./src/navigation/AppNavigator";

function AppInner({ storage }: { storage: IOfflineStorage }): React.ReactElement {
    const config = useMemo(() => loadAppConfig(), []);
    const nodesApi = useMemo(() => new NodesApi(new ApiClient(config), config), [config]);

    const isOnline = useConnectivity(netInfoConnectivity);

    const offlineApi = useMemo(
        () => new OfflineNodesApi(nodesApi, storage, isOnline),
        [nodesApi, storage, isOnline]
    );

    // Keep offlineApi in sync with connectivity
    useEffect(() => {
        offlineApi.setOnline(isOnline);
    }, [offlineApi, isOnline]);

    const syncService = useMemo(() => new SyncService(nodesApi, storage), [nodesApi, storage]);

    const getQueueCount = useCallback(() => storage.getQueueCount(), [storage]);
    useSync(syncService, isOnline, getQueueCount);

    const contextValue = useMemo(
        () => ({ offlineApi, syncService, storage, isOnline }),
        [offlineApi, syncService, storage, isOnline]
    );

    return (
        <OfflineContext.Provider value={contextValue}>
            <NavigationContainer>
                <AppNavigator />
            </NavigationContainer>
        </OfflineContext.Provider>
    );
}

export default function App(): React.ReactElement {
    const [storage, setStorage] = useState<IOfflineStorage | null>(null);

    useEffect(() => {
        getStorage().then(setStorage);
    }, []);

    if (!storage) {
        return (
            <View style={{ flex: 1, justifyContent: "center", alignItems: "center" }}>
                <ActivityIndicator size="large" />
            </View>
        );
    }

    return <AppInner storage={storage} />;
}
