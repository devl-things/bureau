import React from "react";
import { createRoot } from "react-dom/client";
import { NodesAdminApp } from "./NodesAdminApp";

export function boot(elementId: string = "nodes-admin-root"): void {
    const host = document.getElementById(elementId);

    if (!host) {
        throw new Error(`Nodes admin root element "#${elementId}" not found.`);
    }

    createRoot(host).render(
        <React.StrictMode>
            <NodesAdminApp />
        </React.StrictMode>
    );
}
