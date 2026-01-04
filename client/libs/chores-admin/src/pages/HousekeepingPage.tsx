import React, { useEffect, useMemo, useState } from "react";
import type { PagedMeta } from "@bureau/client-core";
import type { HousekeepingChoreDto, HousekeepingLogDto, Props } from "../api/choresApi";
import { ChoresApi } from "../api/choresApi";
import { TableBodyRows } from "../components/TableBodyRows";
import { ModalShell } from "../components/ModalShell";
import { toErrorMessage } from "../utils/ui";


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

export function HousekeepingPage(props: Readonly<Props>): React.ReactElement {
    const choresApi = useMemo(() => new ChoresApi(props.api, props.runtime), [props.api, props.runtime]);

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

    async function load(): Promise<void> {
        setLoading(true);
        setError(null);

        try {
            const result = await choresApi.getHousekeepingLogsAsync({
                page,
                pageSize,
                search: search.trim()
            });

            setItems(result.data ?? []);
            setMeta(result.meta ?? null);
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
            completedChoreIds: (log.completedChores ?? []).map((x: HousekeepingChoreDto) => x.id).filter(Boolean).join(", ")
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
                await choresApi.createHousekeepingLogAsync(payload);
            } else if (modal === "edit" && active?.id) {
                await choresApi.updateHousekeepingLogAsync(active.id, payload);
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
            await choresApi.deleteHousekeepingLogAsync(active.id);
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
                        <TableBodyRows
                            loading={loading}
                            items={items}
                            search={search}
                            colSpan={5}
                            loadingText="Loading logs…"
                            emptyText="No housekeeping logs available."
                            emptyWhenSearchingText="No logs match that search."
                            renderRows={(rows) =>
                                rows.map((log) => {
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
                            }
                        />
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
                            Showing {(meta.page - 1) * meta.pageSize + 1}-{Math.min(meta.page * meta.pageSize, meta.total)} of {meta.total}
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

            <ModalShell open={modal !== "none"} busy={busy} onRequestClose={closeModal} widthStyle={{ width: "min(520px, 100%)" }}>
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
                                <span>Datetime (ISO)</span>
                                <input value={form.datetime} onChange={(e) => setForm({ ...form, datetime: e.target.value })} />
                            </label>

                            <label>
                                <span>Duration (HH:mm)</span>
                                <input value={form.duration} onChange={(e) => setForm({ ...form, duration: e.target.value })} />
                            </label>

                            <label>
                                <span>Note</span>
                                <textarea value={form.note} onChange={(e) => setForm({ ...form, note: e.target.value })} />
                            </label>

                            <label>
                                <span>Completed chore IDs (comma-separated)</span>
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
                            <p style={{ color: "var(--muted)" }}>Are you sure you want to delete log "{formatDate(active.datetime)}"?</p>
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
            </ModalShell>
        </>
    );
}
