import React, { useEffect, useMemo, useState } from "react";
import type { ApiClient } from "@bureau/client-core";
import type { AppRuntimeOptions } from "@bureau/client-core";
import { Endpoints } from "@bureau/client-core";
import { ApiError } from "@bureau/client-core";
import type { PagedMeta, PagedResponse } from "@bureau/client-core/api/Paging";
import { isPagedResponse } from "@bureau/client-core/api/isPagedResponse";

type Props = {
    api: ApiClient;
    runtime: AppRuntimeOptions;
};

type HousekeepingChoreDto = {
    id?: string | null;
    title?: string | null;
};

type HousekeepingLogDto = {
    id: string;
    datetime: string;
    duration: string;
    note?: string | null;
    completedChores?: HousekeepingChoreDto[] | null;
};

type ModalMode = "none" | "view" | "create" | "edit" | "delete";

type LogForm = {
    datetime: string; // ISO
    duration: string; // "HH:mm"
    note: string;
    completedChoreIds: string; // comma-separated for admin (simple)
};

function emptyLogForm(): LogForm {
    return { datetime: new Date().toISOString(), duration: "00:30", note: "", completedChoreIds: "" };
}

function toErrorMessage(err: unknown): string {
    if (err instanceof ApiError) return err.message;
    if (err instanceof Error) return err.message;
    return "Unexpected error";
}

function formatDate(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return new Intl.DateTimeFormat("en-US", {
        weekday: "short",
        month: "short",
        day: "numeric",
        year: "numeric"
    }).format(date);
}

function formatDuration(duration: string): string {
    if (!duration) return "—";
    const parts = duration.toString().split(":").map(Number);
    if (parts.length < 2) return duration;
    const hrs = parts[0] || 0;
    const mins = parts[1] || 0;
    if (!hrs && !mins) return "—";
    if (!hrs) return `${mins} min`;
    if (!mins) return `${hrs} hr${hrs > 1 ? "s" : ""}`;
    return `${hrs} hr${hrs > 1 ? "s" : ""} ${mins} min`;
}

export function HousekeepingPage(props: Props): React.ReactElement {
    const endpoints = useMemo(() => new Endpoints(props.runtime), [props.runtime]);

    const listUrlKey = "chores.housekeeping";
    const byIdUrlKey = "chores.housekeepingById"; // if you have it; if not, we’ll fallback to `${base}/{id}`

    const [items, setItems] = useState<HousekeepingLogDto[]>([]);
    const [meta, setMeta] = useState<PagedMeta | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [search, setSearch] = useState<string>("");
    const [page, setPage] = useState<number>(1);
    const pageSize = 5;

    const [modal, setModal] = useState<ModalMode>("none");
    const [active, setActive] = useState<HousekeepingLogDto | null>(null);
    const [form, setForm] = useState<LogForm>(emptyLogForm());
    const [busy, setBusy] = useState<boolean>(false);

    useEffect(() => {
        void load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, pageSize, search]);

    function getLogByIdUrl(id: string): string {
        const dict = props.runtime.apiBaseUrls;
        if (dict[byIdUrlKey]) {
            return endpoints.build(byIdUrlKey, { id });
        }

        // fallback: list endpoint + /{id}
        const base = endpoints.get(listUrlKey);
        return `${base.replace(/\/+$/, "")}/${encodeURIComponent(id)}`;
    }

    async function load(): Promise<void> {
        setLoading(true);
        setError(null);

        try {
            const url = endpoints.build(listUrlKey, undefined, {
                page,
                pageSize,
                search: search.trim()
            });

            const payload = await props.api.getAsync<unknown>({ path: url });

            if (Array.isArray(payload)) {
                setItems(payload as HousekeepingLogDto[]);
                setMeta(null);
            } else if (isPagedResponse<HousekeepingLogDto>(payload)) {
                const paged = payload as PagedResponse<HousekeepingLogDto>;
                setItems(paged.data ?? []);
                setMeta(paged.meta ?? null);
            } else {
                setItems([]);
                setMeta(null);
            }
        } catch (e: unknown) {
            setItems([]);
            setMeta(null);
            setError(toErrorMessage(e));
        } finally {
            setLoading(false);
        }
    }

    function closeModal(): void {
        if (busy) return;
        setModal("none");
        setActive(null);
        setBusy(false);
    }

    function openView(log: HousekeepingLogDto): void {
        setActive(log);
        setModal("view");
    }

    function openCreate(): void {
        setActive(null);
        setForm(emptyLogForm());
        setModal("create");
    }

    function openEdit(log: HousekeepingLogDto): void {
        setActive(log);
        setForm({
            datetime: log.datetime,
            duration: log.duration,
            note: log.note ?? "",
            completedChoreIds: (log.completedChores ?? []).map((x) => x.id).filter(Boolean).join(", ")
        });
        setModal("edit");
    }

    function openDelete(log: HousekeepingLogDto): void {
        setActive(log);
        setModal("delete");
    }

    async function submitCreateOrEdit(): Promise<void> {
        setBusy(true);
        setError(null);

        try {
            const completedIds = form.completedChoreIds
                .split(",")
                .map((x) => x.trim())
                .filter((x) => x.length > 0);

            const payload = {
                datetime: form.datetime,
                duration: form.duration,
                note: form.note.trim().length > 0 ? form.note.trim() : null,
                completedChoreIds: completedIds
            };

            if (modal === "create") {
                const url = endpoints.get("chores.housekeepingSubmit"); // or a dedicated create endpoint if you have one
                await props.api.postAsync<void>({ path: url, body: payload });
            } else if (modal === "edit" && active?.id) {
                const url = getLogByIdUrl(active.id);
                await props.api.requestAsync<void>({ method: "PUT", path: url, body: { id: active.id, ...payload } });
            }

            closeModal();
            await load();
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }

    async function confirmDelete(): Promise<void> {
        if (!active?.id) return;

        setBusy(true);
        setError(null);

        try {
            const url = getLogByIdUrl(active.id);
            await props.api.requestAsync<void>({ method: "DELETE", path: url });
            closeModal();
            await load();
        } catch (e: unknown) {
            setError(toErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }

    return (
        <>
            <div className="controls" style={{ marginBottom: 18 }}>
                <input
                    type="search"
                    value={search}
                    onChange={(e) => {
                        setSearch(e.target.value);
                        setPage(1);
                    }}
                    placeholder="Search by date, notes or chore…"
                    autoComplete="off"
                />
                <button type="button" onClick={openCreate}>
                    Create log
                </button>
            </div>

            <section className="panel">
                <table>
                    <thead>
                        <tr>
                            <th style={{ width: 180 }}>Date</th>
                            <th style={{ width: 140 }}>Duration</th>
                            <th>Notes</th>
                            <th style={{ width: 140 }}>Chores done</th>
                            <th style={{ width: 220 }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr>
                                <td colSpan={5} className="empty">
                                    Loading logs…
                                </td>
                            </tr>
                        ) : items.length === 0 ? (
                            <tr>
                                <td colSpan={5} className="empty">
                                    {search.trim().length > 0 ? "No logs match that search." : "No housekeeping logs available."}
                                </td>
                            </tr>
                        ) : (
                            items.map((log) => {
                                const count = (log.completedChores ?? []).length;
                                return (
                                    <tr key={log.id}>
                                        <td style={{ padding: "16px 20px" }}>{formatDate(log.datetime)}</td>
                                        <td style={{ padding: "16px 20px" }}>{formatDuration(log.duration)}</td>
                                        <td style={{ padding: "16px 20px", color: "var(--muted)" }}>{log.note ?? "—"}</td>
                                        <td style={{ padding: "16px 20px" }}>
                                            <span className="count-pill">{count}</span>
                                        </td>
                                        <td style={{ padding: "16px 20px" }}>
                                            <div className="actions">
                                                <button type="button" className="btn" onClick={() => openView(log)}>
                                                    View
                                                </button>
                                                <button type="button" className="btn" onClick={() => openEdit(log)}>
                                                    Edit
                                                </button>
                                                <button type="button" className="btn btn-danger" onClick={() => openDelete(log)}>
                                                    Delete
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })
                        )}
                    </tbody>
                </table>

                {meta ? (
                    <div
                        style={{
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "center",
                            padding: "16px 20px",
                            borderTop: "1px solid var(--border)"
                        }}
                    >
                        <div style={{ color: "var(--muted)", fontSize: "0.85rem" }}>
                            Showing {(meta.page - 1) * meta.pageSize + 1}-{Math.min(meta.page * meta.pageSize, meta.total)} of{" "}
                            {meta.total}
                        </div>
                        <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                            <button type="button" className="btn ghost" disabled={!meta.hasPrevious} onClick={() => setPage(page - 1)}>
                                Previous
                            </button>
                            <span style={{ color: "var(--muted)", fontSize: "0.85rem" }}>
                                Page {meta.page} of {meta.totalPages}
                            </span>
                            <button type="button" className="btn ghost" disabled={!meta.hasNext} onClick={() => setPage(page + 1)}>
                                Next
                            </button>
                        </div>
                    </div>
                ) : null}
            </section>

            {error ? <div style={{ marginTop: 12, color: "var(--danger)" }}>{error}</div> : null}

            {/* Modal */}
            <div className={`modal-backdrop ${modal !== "none" ? "visible" : ""}`} aria-hidden={modal === "none"}>
                <div className="modal" style={{ width: "min(520px, 100%)" }}>
                    {modal === "view" && active ? (
                        <>
                            <header>
                                <h2>{formatDate(active.datetime)}</h2>
                                <p style={{ color: "var(--muted)" }}>{active.note ?? "No notes provided"}</p>
                            </header>

                            <ul className="detail-list">
                                <li>
                                    <span className="detail-label">Duration</span>
                                    <span>{formatDuration(active.duration)}</span>
                                </li>
                                <li>
                                    <span className="detail-label">Chores completed</span>
                                    <span>{(active.completedChores ?? []).length}</span>
                                </li>
                            </ul>

                            <section>
                                <h3 style={{ margin: 0 }}>Completed chores</h3>
                                <p className="notes" style={{ margin: "4px 0 8px" }}>
                                    {(active.completedChores ?? []).length ? "List of completed chores." : "No chore data available."}
                                </p>
                                <ul className="chores-list">
                                    {(active.completedChores ?? []).length ? (
                                        (active.completedChores ?? []).map((c, idx) => (
                                            <li key={`${c.id ?? "x"}-${idx}`}>
                                                <strong>{c.title ?? c.id ?? "Chore"}</strong>
                                            </li>
                                        ))
                                    ) : (
                                        <li style={{ listStyle: "none", border: "none", background: "transparent", padding: 0, color: "var(--muted)" }}>
                                            No chores recorded.
                                        </li>
                                    )}
                                </ul>
                            </section>

                            <div className="modal-actions">
                                <button type="button" className="btn" onClick={closeModal}>
                                    Close
                                </button>
                            </div>
                        </>
                    ) : null}

                    {modal === "create" || modal === "edit" ? (
                        <>
                            <header>
                                <h2>{modal === "edit" ? "Edit log" : "Create log"}</h2>
                                <p style={{ color: "var(--muted)" }}>Admin form (simple). Duration format: HH:mm</p>
                            </header>

                            <form
                                onSubmit={(e) => {
                                    e.preventDefault();
                                    void submitCreateOrEdit();
                                }}
                                style={{ display: "flex", flexDirection: "column", gap: 14 }}
                            >
                                <label>
                                    Datetime (ISO)
                                    <input value={form.datetime} onChange={(e) => setForm({ ...form, datetime: e.target.value })} />
                                </label>

                                <label>
                                    Duration (HH:mm)
                                    <input value={form.duration} onChange={(e) => setForm({ ...form, duration: e.target.value })} />
                                </label>

                                <label>
                                    Note
                                    <textarea value={form.note} onChange={(e) => setForm({ ...form, note: e.target.value })} />
                                </label>

                                <label>
                                    Completed chore IDs (comma-separated)
                                    <input
                                        value={form.completedChoreIds}
                                        onChange={(e) => setForm({ ...form, completedChoreIds: e.target.value })}
                                        placeholder="e.g. 2b3a..., 7a1c..."
                                    />
                                </label>

                                <div className="modal-actions">
                                    <button type="button" className="btn ghost" disabled={busy} onClick={closeModal}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn" disabled={busy}>
                                        Save
                                    </button>
                                </div>
                            </form>
                        </>
                    ) : null}

                    {modal === "delete" && active ? (
                        <>
                            <header>
                                <h2>Delete log</h2>
                                <p style={{ color: "var(--muted)" }}>
                                    Are you sure you want to delete log "{formatDate(active.datetime)}"?
                                </p>
                            </header>

                            <div className="modal-actions">
                                <button type="button" className="btn ghost" disabled={busy} onClick={closeModal}>
                                    Cancel
                                </button>
                                <button type="button" className="btn btn-danger" disabled={busy} onClick={() => void confirmDelete()}>
                                    Delete
                                </button>
                            </div>
                        </>
                    ) : null}
                </div>
            </div>
        </>
    );
}
