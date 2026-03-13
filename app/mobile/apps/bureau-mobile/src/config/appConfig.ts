import type { AppRuntimeOptions } from "@bureau/client-core";

export function loadAppConfig(): AppRuntimeOptions {
    return {
        environment: "dev",
        version: "0.1.0",
        apiBaseUrls: {
            nodes: "http://192.168.1.233:5272",
        },
        defaultApiKey: "nodes",
    };
}
