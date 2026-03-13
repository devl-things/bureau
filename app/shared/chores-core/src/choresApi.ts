import type { ApiClient, PagedResponse } from "@bureau/client-core";
import { isPagedResponse } from "@bureau/client-core";
import type {
    ChoreDto,
    ChoreUpsert,
    CriticalPayload,
    HousekeepingLogDto,
    HousekeepingUpsert,
    PrioritizedChoreDto,
    SearchQuery,
} from "./types";
import type { PagedMeta } from "@bureau/client-core";

const EMPTY_META: PagedMeta = {
    page: 0, pageSize: 0, total: 0, totalPages: 0, hasNext: false, hasPrevious: false,
};

function emptyPaged<T>(): PagedResponse<T> {
    return { data: [], meta: EMPTY_META };
}

export class ChoresApi {
    private readonly _api: ApiClient;
    private readonly _base: string;

    public constructor(api: ApiClient, baseUrl: string) {
        this._api = api;
        this._base = baseUrl.replace(/\/$/, "");
    }

    // --- Chores (admin) ---

    public async getChores(query: SearchQuery): Promise<PagedResponse<ChoreDto>> {
        const params = new URLSearchParams({
            page: String(query.page),
            pageSize: String(query.pageSize),
        });
        if (query.search?.trim()) params.set("search", query.search.trim());

        const payload = await this._api.getAsync<unknown>({
            path: `${this._base}/chores?${params}`,
        });
        return isPagedResponse<ChoreDto>(payload) ? payload : emptyPaged<ChoreDto>();
    }

    public async createChore(model: ChoreUpsert): Promise<void> {
        await this._api.postAsync<void>({ path: `${this._base}/chores`, body: model });
    }

    public async updateChore(id: string, model: ChoreUpsert): Promise<void> {
        await this._api.requestAsync<void>({
            method: "PUT",
            path: `${this._base}/chores/${encodeURIComponent(id)}`,
            body: { id, ...model },
        });
    }

    public async deleteChore(id: string): Promise<void> {
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: `${this._base}/chores/${encodeURIComponent(id)}`,
        });
    }

    public async markCritical(id: string, payload: CriticalPayload): Promise<void> {
        await this._api.postAsync<void>({
            path: `${this._base}/chores/${encodeURIComponent(id)}/critical`,
            body: payload,
        });
    }

    public async removeCritical(id: string): Promise<void> {
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: `${this._base}/chores/${encodeURIComponent(id)}/critical`,
        });
    }

    // --- Housekeeping (admin) ---

    public async getHousekeepingLogs(query: SearchQuery): Promise<PagedResponse<HousekeepingLogDto>> {
        const params = new URLSearchParams({
            page: String(query.page),
            pageSize: String(query.pageSize),
        });
        if (query.search?.trim()) params.set("search", query.search.trim());

        const payload = await this._api.getAsync<unknown>({
            path: `${this._base}/housekeeping?${params}`,
        });
        return isPagedResponse<HousekeepingLogDto>(payload) ? payload : emptyPaged<HousekeepingLogDto>();
    }

    public async createHousekeepingLog(model: HousekeepingUpsert): Promise<void> {
        await this._api.postAsync<void>({ path: `${this._base}/housekeeping`, body: model });
    }

    public async updateHousekeepingLog(id: string, model: HousekeepingUpsert): Promise<void> {
        await this._api.requestAsync<void>({
            method: "PUT",
            path: `${this._base}/housekeeping/${encodeURIComponent(id)}`,
            body: { id, ...model },
        });
    }

    public async deleteHousekeepingLog(id: string): Promise<void> {
        await this._api.requestAsync<void>({
            method: "DELETE",
            path: `${this._base}/housekeeping/${encodeURIComponent(id)}`,
        });
    }

    // --- Daily chores (consumer) ---

    public async getPrioritizedChores(date: string): Promise<PrioritizedChoreDto[]> {
        const payload = await this._api.getAsync<{ data: PrioritizedChoreDto[] }>({
            path: `${this._base}/housekeeping/prioritized-chores?date=${encodeURIComponent(date)}`,
        });
        return Array.isArray(payload?.data) ? payload.data : [];
    }

    public async submitHousekeeping(model: HousekeepingUpsert): Promise<void> {
        await this._api.postAsync<void>({
            path: `${this._base}/housekeeping/submit`,
            body: model,
        });
    }

    // --- Health ---

    public async checkHealth(timeoutMs = 3000): Promise<boolean> {
        const controller = new AbortController();
        const timer = setTimeout(() => controller.abort(), timeoutMs);
        try {
            const resp = await fetch(`${this._base}/health`, { signal: controller.signal });
            return resp.ok;
        } catch {
            return false;
        } finally {
            clearTimeout(timer);
        }
    }
}
