import type { AppRuntimeOptions, PagedMeta, PagedResponse } from "@bureau/client-core";
import { ApiClient, Endpoints, isPagedResponse } from "@bureau/client-core";


export type Props = {
    api: ApiClient;
    runtime: AppRuntimeOptions;
};

export type SearchQuery = {
    page: number;
    pageSize: number;
    search?: string;
};

// Chores
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

// Housekeeping

export type HousekeepingChoreDto = {
    id?: string | null;
    title?: string | null;
};

export type HousekeepingLogDto = {
    id: string;
    datetime: string;
    duration: string;
    note?: string | null;
    completedChores?: HousekeepingChoreDto[] | null;
};

export type HousekeepingUpsert = {
    datetime: string; // ISO
    duration: string; // "HH:mm" or whatever your API expects
    note: string | null;
    completedChoreIds: string[];
};

// -----------------------------
// Shared helpers
// -----------------------------
const EMPTY_PAGED_META: PagedMeta = {
    page: 0,
    pageSize: 0,
    total: 0,
    totalPages: 0,
    hasNext: false,
    hasPrevious: false
};

function emptyPagedResponse<T>(): PagedResponse<T> {
    return { data: [], meta: EMPTY_PAGED_META };
}

export class ChoresApi {
    private readonly _api: ApiClient;
    private readonly _endpoints: Endpoints;

    public constructor(api: ApiClient, runtime: AppRuntimeOptions) {
        this._api = api;
        this._endpoints = new Endpoints(runtime);
    }

    // Chores
    public async getChoresAsync(input: SearchQuery): Promise<PagedResponse<ChoreDto>> {
        const url = this._endpoints.build("chores.chores", undefined, {
            page: input.page,
            pageSize: input.pageSize,
            search: (input.search ?? "").trim()
        });

        const payload: unknown = await this._api.getAsync<unknown>({ path: url });

        if (isPagedResponse<ChoreDto>(payload)) {
            return payload;
        }

        return emptyPagedResponse<ChoreDto>();
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

    // Housekeeping
    public async getHousekeepingLogsAsync(input: SearchQuery): Promise<PagedResponse<HousekeepingLogDto>> {
        const url = this._endpoints.build("chores.housekeeping", undefined, {
            page: input.page,
            pageSize: input.pageSize,
            search: (input.search ?? "").trim()
        });

        const payload: unknown = await this._api.getAsync<unknown>({ path: url });

        if (isPagedResponse<HousekeepingLogDto>(payload)) {
            return payload;
        }

        return emptyPagedResponse<HousekeepingLogDto>();
    }

    public async createHousekeepingLogAsync(model: HousekeepingUpsert): Promise<void> {
        // your page used endpoints.get("chores.housekeepingSubmit") for create
        const url = this._endpoints.get("chores.housekeepingSubmit");
        await this._api.postAsync<void>({ path: url, body: model });
    }

    public async updateHousekeepingLogAsync(id: string, model: HousekeepingUpsert): Promise<void> {
        const url = this._endpoints.build("chores.housekeepingById", { id });
        await this._api.requestAsync<void>({
            method: "PUT",
            path: url,
            body: { id, ...model }
        });
    }

    public async deleteHousekeepingLogAsync(id: string): Promise<void> {
        const url = this._endpoints.build("chores.housekeepingById", { id });
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: url
        });
    }
}
