import type { AppRuntimeOptions } from "./AppRuntimeOptions";

declare global {
    interface Window {
        __BUREAU__?: unknown;
    }
}

export class AppRuntimeOptionsLoader {
    public static loadFromWindow(): AppRuntimeOptions {
        const raw: unknown = window.__BUREAU__;

        if (raw === null || raw === undefined || typeof raw !== "object") {
            throw new Error("Missing window.__BUREAU__ runtime configuration.");
        }

        const obj: Record<string, unknown> = raw as Record<string, unknown>;

        const environment: unknown = obj["environment"];
        const version: unknown = obj["version"];
        const apiBaseUrls: unknown = obj["apiBaseUrls"];
        const defaultApiKey: unknown = obj["defaultApiKey"];

        if (typeof environment !== "string" || environment.trim().length === 0) {
            throw new Error("Invalid runtime config: 'environment' is required.");
        }

        if (typeof version !== "string" || version.trim().length === 0) {
            throw new Error("Invalid runtime config: 'version' is required.");
        }

        if (apiBaseUrls === null || apiBaseUrls === undefined || typeof apiBaseUrls !== "object") {
            throw new Error("Invalid runtime config: 'apiBaseUrls' is required.");
        }

        const urls: Record<string, string> = {};
        const dict: Record<string, unknown> = apiBaseUrls as Record<string, unknown>;
        for (const key of Object.keys(dict)) {
            const value: unknown = dict[key];
            if (typeof value === "string" && value.trim().length > 0) {
                urls[key] = value;
            }
        }

        if (Object.keys(urls).length === 0) {
            throw new Error("Invalid runtime config: 'apiBaseUrls' must contain at least one entry.");
        }

        const config: AppRuntimeOptions = {
            environment: environment.trim(),
            version: version.trim(),
            apiBaseUrls: urls
        };

        if (typeof defaultApiKey === "string" && defaultApiKey.trim().length > 0) {
            config.defaultApiKey = defaultApiKey.trim();
        }

        return config;
    }
}
