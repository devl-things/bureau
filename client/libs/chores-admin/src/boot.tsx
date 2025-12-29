import React from "react";
import { createRoot } from "react-dom/client";
import { ChoresAdminApp } from "./ChoresAdminApp";

export function boot(elementId: string = "chores-admin-root"): void {
    const host = document.getElementById(elementId);

    if (!host) {
        throw new Error(`Chores admin root element "#${elementId}" not found.`);
    }

    createRoot(host).render(
        <React.StrictMode>
            <ChoresAdminApp />
        </React.StrictMode>
    );
}
