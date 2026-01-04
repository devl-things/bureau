import type { AppRuntimeOptions } from "../config/AppRuntimeOptions";

export type ParamValue = string | number | boolean | null | undefined;
export type RouteParams = Record<string, ParamValue>;
export type QueryParams = Record<string, ParamValue>;

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

        url = this.applyRouteParams(url, route);

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

    private applyRouteParams(url: string, route?: RouteParams): string {
        if (!route) {
            return url;
        }

        let result: string = url;

        for (const routeKey of Object.keys(route)) {
            const token: string = `{${routeKey}}`;
            const raw: ParamValue = route[routeKey];
            const replaced: string = raw === null || raw === undefined ? "" : encodeURIComponent(String(raw));
            result = result.split(token).join(replaced);
        }

        return result;
    }
}
