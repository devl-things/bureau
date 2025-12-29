import type { AppRuntimeOptions } from "../config/AppRuntimeOptions";

export type RouteParams = Record<string, string | number | boolean | null | undefined>;
export type QueryParams = Record<string, string | number | boolean | null | undefined>;

export class Endpoints {
    private readonly _urls: Record<string, string>;

    public constructor(config: AppRuntimeOptions) {
        this._urls = config.apiBaseUrls;
    }

    public get(key: string): string {
        const value: string | undefined = this._urls[key];
        if (value === undefined || value.trim().length === 0) {
            throw new Error(`Endpoints: missing key '${key}' in runtime config.`);
        }
        return value.trim();
    }

    public build(key: string, route?: RouteParams, query?: QueryParams): string {
        let url: string = this.get(key);

        if (route) {
            for (const routeKey of Object.keys(route)) {
                const token = `{${routeKey}}`;
                const raw = route[routeKey];
                const replaced = raw === null || raw === undefined ? "" : encodeURIComponent(String(raw));
                url = url.split(token).join(replaced);
            }
        }

        if (query) {
            const qs = new URLSearchParams();
            for (const qKey of Object.keys(query)) {
                const v = query[qKey];
                if (v === null || v === undefined) continue;
                const s = String(v).trim();
                if (s.length === 0) continue;
                qs.set(qKey, s);
            }

            const queryString = qs.toString();
            if (queryString.length > 0) {
                url = url.includes("?") ? `${url}&${queryString}` : `${url}?${queryString}`;
            }
        }

        return url;
    }
}
