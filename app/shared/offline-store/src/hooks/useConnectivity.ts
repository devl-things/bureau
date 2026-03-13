import { useEffect, useState } from "react";

export interface ConnectivityProvider {
    subscribe(cb: (isOnline: boolean) => void): () => void;
    getCurrentState(): Promise<boolean>;
}

export function useConnectivity(provider: ConnectivityProvider): boolean {
    const [isOnline, setIsOnline] = useState(true);

    useEffect(() => {
        let mounted = true;

        provider.getCurrentState().then((state) => {
            if (mounted) setIsOnline(state);
        });

        const unsubscribe = provider.subscribe((online) => {
            if (mounted) setIsOnline(online);
        });

        return () => {
            mounted = false;
            unsubscribe();
        };
    }, [provider]);

    return isOnline;
}
