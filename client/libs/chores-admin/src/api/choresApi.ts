import type { AppRuntimeOptions, PagedMeta, PagedResponse } from "@bureau/client-core";
import { ApiClient, Endpoints, isPagedResponse } from "@bureau/client-core";


export type Props = {
    api: ApiClient;
    runtime: AppRuntimeOptions;
};

export type ChoreDto = {
    id: string;
    title: string;
    description?: string | null;
    type?: string | null;
    weeklyInterval?: number | null;
    isCritical?: boolean;
};

export type ChoreUpsert = {
    title: string;
    description: string | null;
    type: string;
    weeklyInterval: number;
};

export type CriticalPayload = {
    description: string;
};

export type PagedResult<T> = {
    items: T[];
    meta: PagedMeta | null;
};

export class ChoresApi {
    private readonly _api: ApiClient;
    private readonly _endpoints: Endpoints;

    public constructor(api: ApiClient, runtime: AppRuntimeOptions) {
        this._api = api;
        this._endpoints = new Endpoints(runtime);
    }

    public async getChoresAsync(input: { page: number; pageSize: number; search?: string }): Promise<PagedResult<ChoreDto>> {
        const url = this._endpoints.build("chores.chores", undefined, {
            page: input.page,
            pageSize: input.pageSize,
            search: (input.search ?? "").trim()
        });

        const payload: unknown = await this._api.getAsync<unknown>({ path: url });

        if (Array.isArray(payload)) {
            return { items: payload as ChoreDto[], meta: null };
        }

        if (isPagedResponse<ChoreDto>(payload)) {
            return { items: payload.data ?? [], meta: payload.meta ?? null };
        }

        return { items: [], meta: null };
    }

    public async createChoreAsync(model: ChoreUpsert): Promise<void> {
        const url = this._endpoints.get("chores.chores");
        await this._api.postAsync<void>({ path: url, body: model });
    }

    public async updateChoreAsync(id: string, model: ChoreUpsert): Promise<void> {
        const url = this._endpoints.build("chores.choresById", { id });
        await this._api.requestAsync<void>({
            method: "PUT",
            path: url,
            body: { id, ...model }
        });
    }

    public async deleteChoreAsync(id: string): Promise<void> {
        const url = this._endpoints.build("chores.choresById", { id });
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: url
        });
    }

    public async markCriticalAsync(id: string, payload: CriticalPayload): Promise<void> {
        const url = this._endpoints.build("chores.critical", { id });
        await this._api.postAsync<void>({
            path: url,
            body: payload
        });
    }

    public async removeCriticalAsync(id: string): Promise<void> {
        const url = this._endpoints.build("chores.critical", { id });
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: url
        });
    }
}
