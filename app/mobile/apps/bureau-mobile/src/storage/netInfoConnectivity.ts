import NetInfo from "@react-native-community/netinfo";
import type { ConnectivityProvider } from "@bureau/offline-store";

export const netInfoConnectivity: ConnectivityProvider = {
    subscribe(cb: (isOnline: boolean) => void): () => void {
        const unsubscribe = NetInfo.addEventListener((state) => {
            cb(state.isConnected ?? false);
        });
        return unsubscribe;
    },

    async getCurrentState(): Promise<boolean> {
        const state = await NetInfo.fetch();
        return state.isConnected ?? false;
    },
};
