import type { PagedResponse } from "./Paging";

export function isPagedResponse<T>(value: unknown): value is PagedResponse<T> {
    if (value === null || value === undefined || typeof value !== "object") return false;

    const obj: Record<string, unknown> = value as Record<string, unknown>;
    const hasData: boolean = Array.isArray(obj["data"]);
    const meta: unknown = obj["meta"];

    if (!hasData || meta === null || meta === undefined || typeof meta !== "object") return false;

    const m: Record<string, unknown> = meta as Record<string, unknown>;
    return (
        typeof m["page"] === "number" &&
        typeof m["pageSize"] === "number" &&
        typeof m["total"] === "number" &&
        typeof m["totalPages"] === "number" &&
        typeof m["hasNext"] === "boolean" &&
        typeof m["hasPrevious"] === "boolean"
    );
}
