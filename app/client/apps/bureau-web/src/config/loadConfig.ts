import type { AppConfig } from "./AppConfig";

export async function loadConfig(): Promise<AppConfig> {
    const res = await fetch("/config.json");
    if (!res.ok) {
        throw new Error(`Failed to load config.json: ${res.status} ${res.statusText}`);
    }
    return res.json() as Promise<AppConfig>;
}
