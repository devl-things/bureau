import type { AppRuntimeOptions } from "../config/AppRuntimeOptions";
import type { IAccessTokenProvider } from "../auth/IAccessTokenProvider";
import { NullAccessTokenProvider } from "../auth/NullAccessTokenProvider";
import { AuthorizationHeaderFactory } from "../auth/AuthorizationHeaderFactory";
import type { ProblemDetails } from "./ProblemDetails";
import { ApiError } from "./ApiError";
import { isJsonContentType } from "./contentType";

export type ApiRequest = {
    apiKey?: string;
    path: string; // "/api/etl/jobs"
    method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
    body?: unknown;
    headers?: Record<string, string>;
    signal?: AbortSignal;
    credentials?: RequestCredentials;
};

export class ApiClient {
    private readonly _config: AppRuntimeOptions;
    private readonly _tokenProvider: IAccessTokenProvider;

    public constructor(config: AppRuntimeOptions, tokenProvider?: IAccessTokenProvider) {
        this._config = config;
        this._tokenProvider = tokenProvider ?? new NullAccessTokenProvider();
    }

    public async getAsync<T>(request: Omit<ApiRequest, "method" | "body">): Promise<T> {
        return await this.requestAsync<T>({ ...request, method: "GET" });
    }

    public async postAsync<T>(request: Omit<ApiRequest, "method">): Promise<T> {
        return await this.requestAsync<T>({ ...request, method: "POST" });
    }

    public async requestAsync<T>(request: ApiRequest): Promise<T> {
        const method: string = request.method ?? "GET";
        const baseUrl: string = this.resolveBaseUrl(request.apiKey);
        const url: string = this.combineUrl(baseUrl, request.path);

        const headers: Record<string, string> = request.headers ?? {};

        const token: string | null = await this._tokenProvider.getAccessTokenAsync();
        const authHeader: string | null = AuthorizationHeaderFactory.createBearer(token);
        if (authHeader !== null) {
            headers["Authorization"] = authHeader;
        }

        let body: BodyInit | undefined = undefined;
        if (request.body !== undefined) {
            if (!("Content-Type" in headers)) {
                headers["Content-Type"] = "application/json";
            }
            body = JSON.stringify(request.body);
        }

        const response: Response = await fetch(url, {
            method,
            headers,
            body,
            signal: request.signal,
            credentials: request.credentials ?? "same-origin"
        });

        if (response.status === 204) {
            return undefined as unknown as T;
        }

        const contentType: string | null = response.headers.get("content-type");
        const requestInfo = { method, url };

        if (!response.ok) {
            throw await this.createApiErrorAsync(response, requestInfo, contentType);
        }

        if (isJsonContentType(contentType)) {
            return (await response.json()) as T;
        }

        // If needed later, add a separate API for text; for now keep this strict.
        const text: string = await response.text();
        return text as unknown as T;
    }

    private resolveBaseUrl(apiKey?: string): string {
        const key: string | undefined = apiKey ?? this._config.defaultApiKey;
        if (key === undefined || key.trim().length === 0) {
            throw new Error("ApiClient: apiKey is required (no defaultApiKey configured).");
        }

        const baseUrl: string | undefined = this._config.apiBaseUrls[key];
        if (baseUrl === undefined || baseUrl.trim().length === 0) {
            throw new Error(`ApiClient: apiBaseUrls does not contain key '${key}'.`);
        }

        return baseUrl.trim();
    }

    private combineUrl(baseUrl: string, path: string): string {
        const trimmedPath: string = path.trim();

        // ✅ allow calling full absolute endpoints from runtime config
        if (trimmedPath.startsWith("http://") || trimmedPath.startsWith("https://")) {
            return trimmedPath;
        }

        const left: string = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
        const right: string = trimmedPath.startsWith("/") ? trimmedPath : `/${trimmedPath}`;
        return `${left}${right}`;
    }

    private async createApiErrorAsync(
        response: Response,
        requestInfo: { method: string; url: string },
        contentType: string | null
    ): Promise<ApiError> {
        const status: number = response.status;

        if (isJsonContentType(contentType)) {
            try {
                const json: unknown = await response.json();
                const problem: ProblemDetails | undefined = this.tryAsProblemDetails(json);
                const message: string = problem?.title ?? problem?.detail ?? `Request failed with status ${status}.`;
                return new ApiError(message, status, requestInfo, problem, undefined);
            } catch {
                // fall through to text
            }
        }

        const bodyText: string = await response.text().catch(() => "");
        const message: string = bodyText.length > 0 ? bodyText : `Request failed with status ${status}.`;
        return new ApiError(message, status, requestInfo, undefined, bodyText);
    }

    private tryAsProblemDetails(value: unknown): ProblemDetails | undefined {
        if (value === null || value === undefined || typeof value !== "object") return undefined;

        const obj: Record<string, unknown> = value as Record<string, unknown>;
        const hasTitle: boolean = typeof obj["title"] === "string";
        const hasStatus: boolean = typeof obj["status"] === "number";
        if (!hasTitle && !hasStatus) return undefined;

        return obj as ProblemDetails;
    }
}
