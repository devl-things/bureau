import React from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ConfigProvider } from "./config/ConfigContext";
import { AuthProvider } from "./auth/AuthContext";
import { App } from "./App";
import { loadConfig } from "./config/loadConfig";
import type { AppConfig } from "./config/AppConfig";

async function main(): Promise<void> {
    const config: AppConfig = await loadConfig();

    // Bridge: populate window.__BUREAU__ so feature modules (chores-admin, nodes-admin)
    // can read config via AppRuntimeOptionsLoader.loadFromWindow().
    // Both ChoresApi and NodesApi now construct paths from their base URL directly.
    (window as Record<string, unknown>)["__BUREAU__"] = {
        environment: config.environment,
        version: "1.0.0",
        defaultApiKey: "nodes",
        apiBaseUrls: {
            chores: config.apis.chores,
            nodes: config.apis.nodes,
        },
    };

    const rootEl = document.getElementById("root");
    if (!rootEl) throw new Error("Root element #root not found in DOM");

    createRoot(rootEl).render(
        <React.StrictMode>
            <ConfigProvider config={config}>
                <AuthProvider config={config}>
                    <BrowserRouter>
                        <App />
                    </BrowserRouter>
                </AuthProvider>
            </ConfigProvider>
        </React.StrictMode>
    );
}

main().catch((err: unknown) => {
    document.body.textContent = `Bureau failed to start: ${String(err)}`;
});
