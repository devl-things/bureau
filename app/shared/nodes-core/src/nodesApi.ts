import type { AppRuntimeOptions } from "@bureau/client-core";
import { ApiClient } from "@bureau/client-core";
import type {
    NodeDto,
    CreateNodeRequest,
    PatchNodeAttributesRequest,
    SearchNodesQuery,
    EdgeDto,
    CreateEdgeRequest,
    RemoveEdgeRequest,
    CursorResponse,
} from "./types";
import type { NodeKindDefinition } from "./definitions";

export class NodesApi {
    private readonly _api: ApiClient;
    private readonly _baseUrl: string;

    public constructor(api: ApiClient, runtime: AppRuntimeOptions) {
        const base = runtime.apiBaseUrls["nodes"] ?? runtime.apiBaseUrls[runtime.defaultApiKey ?? ""];
        if (!base) {
            throw new Error("NodesApi: no 'nodes' key found in apiBaseUrls.");
        }
        this._api = api;
        this._baseUrl = base.endsWith("/") ? base.slice(0, -1) : base;
    }

    public async createNode(
        definition: NodeKindDefinition,
        request: CreateNodeRequest
    ): Promise<NodeDto> {
        return await this._api.postAsync<NodeDto>({
            path: `${this._baseUrl}/${definition.apiRoute}`,
            body: request,
        });
    }

    public async searchNodes(
        definition: NodeKindDefinition,
        query: SearchNodesQuery
    ): Promise<CursorResponse<NodeDto>> {
        const params = new URLSearchParams();
        if (query.query) params.set("query", query.query);
        if (query.scope) params.set("scope", query.scope);
        if (query.locale) params.set("locale", query.locale);
        if (query.cursor) params.set("cursor", query.cursor);
        if (query.limit) params.set("limit", String(query.limit));

        const qs = params.toString();
        const url = `${this._baseUrl}/${definition.apiRoute}${qs ? `?${qs}` : ""}`;

        return await this._api.getAsync<CursorResponse<NodeDto>>({ path: url });
    }

    public async getNode(nodeId: string): Promise<NodeDto> {
        return await this._api.getAsync<NodeDto>({
            path: `${this._baseUrl}/nodes/${nodeId}`,
        });
    }

    public async patchAttributes(
        nodeId: string,
        patch: PatchNodeAttributesRequest
    ): Promise<void> {
        await this._api.requestAsync<void>({
            method: "PATCH",
            path: `${this._baseUrl}/nodes/${nodeId}/attributes`,
            body: patch,
        });
    }

    public async addEdge(nodeId: string, request: CreateEdgeRequest): Promise<void> {
        await this._api.postAsync<void>({
            path: `${this._baseUrl}/nodes/${nodeId}/edges`,
            body: request,
        });
    }

    public async getEdges(
        nodeId: string,
        purpose?: string,
        cursor?: string,
        limit?: number
    ): Promise<CursorResponse<EdgeDto>> {
        const params = new URLSearchParams();
        if (purpose) params.set("purpose", purpose);
        if (cursor) params.set("cursor", cursor);
        if (limit) params.set("limit", String(limit));

        const qs = params.toString();
        const url = `${this._baseUrl}/nodes/${nodeId}/edges${qs ? `?${qs}` : ""}`;

        return await this._api.getAsync<CursorResponse<EdgeDto>>({ path: url });
    }

    public async removeEdge(nodeId: string, request: RemoveEdgeRequest): Promise<void> {
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: `${this._baseUrl}/nodes/${nodeId}/edges`,
            body: request,
        });
    }
}
