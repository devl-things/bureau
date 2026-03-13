import React, { useEffect, useRef, useState } from "react";
import type { PagedMeta } from "@bureau/client-core";
import type { ChoreDto, Props } from "../api/choresApi";
import { TableBodyRows } from "../components/TableBodyRows";
import { ModalShell } from "../components/ModalShell";
import { toErrorMessage } from "../utils/ui";

type ModalMode = "none" | "create" | "edit" | "delete" | "critical" | "critical-edit";

type ChoreUpsertForm = {
    title: string;
    description: string;
    type: string;
    repeatEveryWeeks: string;
};

type CriticalForm = {
    description: string;
};

function emptyChoreForm(): ChoreUpsertForm {
    return { title: "", description: "", type: "Maintenance", repeatEveryWeeks: "1" };
}

function emptyCriticalForm(): CriticalForm {
    return { description: "" };
}

/**
 * Adjust these mappings to match your real API shape.
 * I assume ChoreDto exposes critical fields so we can prefill the modal.
 */
function prefillCriticalFormFromChore(chore: ChoreDto): CriticalForm {
    // description/note
    const rawDesc = ((chore as any).criticalDescription ?? (chore as any).criticalNote) as string | null | undefined;
    const description = typeof rawDesc === "string" ? rawDesc : "";

    return { description };
}

export function ChorePage(props: Readonly<Props>): React.ReactElement {
    const { choresApi } = props;

    const [items, setItems] = useState<ChoreDto[]>([]);
    const [meta, setMeta] = useState<PagedMeta | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [search, setSearch] = useState<string>("");
    const [page, setPage] = useState<number>(1);
    const pageSize = 5;

    const searchDebounceRef = useRef<number | null>(null);

    const [modal, setModal] = useState<ModalMode>("none");
    const [active, setActive] = useState<ChoreDto | null>(null);

    const [choreForm, setChoreForm] = useState<ChoreUpsertForm>(emptyChoreForm());
    const [criticalForm, setCriticalForm] = useState<CriticalForm>(emptyCriticalForm());
    const [modalBusy, setModalBusy] = useState<boolean>(false);
    const [toast, setToast] = useState<{ message: string; positive: boolean } | null>(null);

    useEffect(() => {
        void load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, pageSize, search]);

    async function load(): Promise<void> {
        setLoading(true);
        setError(null);
        try {
            const result = await choresApi.getChores({ page, pageSize, search: search.trim() });
            setItems(result.data);
            setMeta(result.meta);
        } catch (e: unknown) {
            setItems([]);
            setMeta(null);
            setError(toErrorMessage(e));
        } finally {
            setLoading(false);
        }
    }

    function showToast(message: string, positive: boolean): void {
        setToast({ message, positive });
        globalThis.clearTimeout((showToast as any)._t);
        (showToast as any)._t = globalThis.setTimeout(() => setToast(null), 2600);
    }

    function openCreate(): void {
        setActive(null);
        setChoreForm(emptyChoreForm());
        setModal("create");
    }

    function openEdit(chore: ChoreDto): void {
        setActive(chore);
        setChoreForm({
            title: chore.title ?? "",
            description: chore.description ?? "",
            type: chore.type ?? "Maintenance",
            repeatEveryWeeks: String(chore.weeklyInterval ?? 1)
        });
        setModal("edit");
    }

    function openDelete(chore: ChoreDto): void {
        setActive(chore);
        setModal("delete");
    }

    function openCritical(chore: ChoreDto): void {
        setActive(chore);

        if (chore.isCritical) {
            setCriticalForm(prefillCriticalFormFromChore(chore));
            setModal("critical-edit");
            return;
        }

        setCriticalForm(emptyCriticalForm());
        setModal("critical");
    }

    function closeModal(): void {
        if (modalBusy) return;
        setModal("none");
        setActive(null);
        setModalBusy(false);
    }

    function onSearchChange(value: string): void {
        setSearch(value);
        setPage(1);

        if (searchDebounceRef.current) {
            globalThis.clearTimeout(searchDebounceRef.current);
        }

        searchDebounceRef.current = globalThis.setTimeout(() => {
            // state already updated; effect triggers load
        }, 300);
    }

    async function submitChoreForm(): Promise<void> {
        setModalBusy(true);
        setError(null);

        try {
            const title = choreForm.title.trim();
            if (title.length === 0) {
                showToast("Title is required.", false);
                return;
            }

            const weeklyIntervalParsed = Number(choreForm.repeatEveryWeeks);
            const weeklyInterval = Number.isFinite(weeklyIntervalParsed) && weeklyIntervalParsed > 0 ? weeklyIntervalParsed : 1;

            const payload = {
                title,
                description: choreForm.description.trim().length > 0 ? choreForm.description.trim() : null,
                type: choreForm.type.trim().length > 0 ? choreForm.type.trim() : "Maintenance",
                weeklyInterval
            };

            if (modal === "create") {
                await choresApi.createChore(payload);
                showToast("Chore created.", true);
            } else if (modal === "edit" && active?.id) {
                await choresApi.updateChore(active.id, payload);
                showToast("Chore updated.", true);
            }

            closeModal();
            await load();
        } catch (e: unknown) {
            setError(toErrorMessage(e));
            showToast(toErrorMessage(e), false);
        } finally {
            setModalBusy(false);
        }
    }

    async function confirmDelete(): Promise<void> {
        if (!active?.id) return;
        setModalBusy(true);
        setError(null);

        try {
            await choresApi.deleteChore(active.id);
            showToast("Chore deleted.", true);
            closeModal();
            await load();
        } catch (e: unknown) {
            setError(toErrorMessage(e));
            showToast(toErrorMessage(e), false);
        } finally {
            setModalBusy(false);
        }
    }

    async function submitCritical(): Promise<void> {
        if (!active?.id) return;

        setModalBusy(true);
        setError(null);

        try {
            const description = criticalForm.description.trim();

            if (description.length === 0) {
                showToast("Notes are required.", false);
                return;
            }

            // Treat this as upsert: mark or update critical info.
            await choresApi.markCritical(active.id, { description });

            if (modal === "critical-edit") {
                showToast("Critical info updated.", true);
            } else {
                showToast("Marked as critical.", true);
            }

            closeModal();
            await load();
        } catch (e: unknown) {
            setError(toErrorMessage(e));
            showToast(toErrorMessage(e), false);
        } finally {
            setModalBusy(false);
        }
    }

    async function removeCritical(chore: ChoreDto): Promise<void> {
        if (!chore.id) return;

        const ok = globalThis.confirm(`Remove critical status from "${chore.title ?? chore.id}"?`);
        if (!ok) return;

        try {
            await choresApi.removeCritical(chore.id);
            showToast("Critical removed.", true);
            await load();
        } catch (e: unknown) {
            showToast(toErrorMessage(e), false);
        }
    }

    const criticalModalTitle = modal === "critical-edit" ? "Edit critical" : "Mark critical";
    const criticalModalSubtitle =
        modal === "critical-edit"
            ? `Update critical reminder details for "${active?.title ?? active?.id ?? "this chore"}".`
            : `Provide context for why "${active?.title ?? active?.id ?? "this chore"}" is critical.`;

    function renderModalBody(): React.ReactNode {
        if (modal === "create" || modal === "edit") {
            return (
                <>
                    <header>
                        <h2>{modal === "edit" ? "Edit chore" : "Create chore"}</h2>
                        <p>{modal === "edit" ? "Update the selected chore." : "Add a brand new chore."}</p>
                    </header>

                    <form
                        onSubmit={(e) => {
                            e.preventDefault();
                            void submitChoreForm();
                        }}
                        style={{ display: "flex", flexDirection: "column", gap: 14 }}
                    >
                        <label>
                            <span>Title</span>
                            <input
                                value={choreForm.title}
                                required
                                onChange={(e) => setChoreForm({ ...choreForm, title: e.target.value })}
                            />
                        </label>

                        <label>
                            <span>Description</span>
                            <textarea
                                value={choreForm.description}
                                onChange={(e) => setChoreForm({ ...choreForm, description: e.target.value })}
                            />
                        </label>

                        <label>
                            <span>Type</span>
                            <input
                                value={choreForm.type}
                                placeholder="e.g. Cleaning, Errand"
                                onChange={(e) => setChoreForm({ ...choreForm, type: e.target.value })}
                            />
                        </label>

                        <label>
                            <span>Repeat every (weeks)</span>
                            <input
                                type="number"
                                min={1}
                                step={1}
                                value={choreForm.repeatEveryWeeks}
                                onChange={(e) => setChoreForm({ ...choreForm, repeatEveryWeeks: e.target.value })}
                            />
                        </label>

                        <div className="modal-actions">
                            <button type="button" className="btn ghost" disabled={modalBusy} onClick={closeModal}>
                                Cancel
                            </button>
                            <button type="submit" className="btn" disabled={modalBusy}>
                                {modal === "edit" ? "Save changes" : "Create"}
                            </button>
                        </div>
                    </form>
                </>
            );
        }
        if (modal === "delete") {
            return (
                <>
                    <header>
                        <h2>Delete chore</h2>
                        <p>Are you sure you want to delete "{active?.title ?? active?.id ?? "this chore"}"?</p>
                    </header>

                    <div className="modal-actions">
                        <button type="button" className="btn ghost" disabled={modalBusy} onClick={closeModal}>
                            Cancel
                        </button>
                        <button type="button" className="btn btn-danger" disabled={modalBusy} onClick={() => void confirmDelete()}>
                            Delete
                        </button>
                    </div>
                </>
            );
        }

        if (modal === "critical" || modal === "critical-edit") {
            return (
                <>
                    <header>
                        <h2>{criticalModalTitle}</h2>
                        <p>{criticalModalSubtitle}</p>
                    </header>

                    <form
                        onSubmit={(e) => {
                            e.preventDefault();
                            void submitCritical();
                        }}
                        style={{ display: "flex", flexDirection: "column", gap: 14 }}
                    >
                        <label>
                            <span>Notes</span>
                            <textarea
                                required
                                value={criticalForm.description}
                                placeholder="Describe why this is critical"
                                onChange={(e) => setCriticalForm({ ...criticalForm, description: e.target.value })}
                            />
                        </label>

                        <div className="modal-actions">
                            <button type="button" className="btn ghost" disabled={modalBusy} onClick={closeModal}>
                                Cancel
                            </button>
                            <button type="submit" className="btn" disabled={modalBusy}>
                                Save
                            </button>
                        </div>
                    </form>
                </>
            );
        }
    }

    return (
        <>
            <div className="controls" style={{ marginBottom: 18 }}>
                <input
                    type="search"
                    value={search}
                    onChange={(e) => onSearchChange(e.target.value)}
                    placeholder="Search chores by id or title…"
                    autoComplete="off"
                />
                <button type="button" onClick={openCreate}>
                    Create chore
                </button>
            </div>

            <section className="panel">
                <table>
                    <thead>
                        <tr>
                            <th style={{ width: 140 }}>Chore ID</th>
                            <th>Title</th>
                            <th style={{ width: 320 }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <TableBodyRows
                            loading={loading}
                            items={items}
                            search={search}
                            colSpan={3}
                            loadingText="Loading chores…"
                            emptyText="No chores found."
                            emptyWhenSearchingText="No chores match that search."
                            renderRows={(rows) =>
                                rows.map((chore) => (
                                    <tr key={chore.id ?? ""}>
                                        <td style={{ padding: "16px 20px" }}>{chore.id}</td>
                                        <td style={{ padding: "16px 20px" }}>
                                            <div>{chore.title ?? "Untitled"}</div>

                                            <div className="meta-line">
                                                {chore.type ? <span className="badge">{chore.type}</span> : null}
                                                {chore.isCritical ? (
                                                    <span
                                                        className="badge"
                                                        style={{
                                                            background: "rgba(159, 18, 57, 0.16)",
                                                            borderColor: "rgba(159, 18, 57, 0.25)",
                                                            color: "#f87171"
                                                        }}
                                                    >
                                                        Critical
                                                    </span>
                                                ) : null}
                                                {chore.weeklyInterval ? (
                                                    <small className="muted-label">Every {chore.weeklyInterval} wk(s)</small>
                                                ) : null}
                                            </div>

                                            {chore.description ? <small className="badge subtle">{chore.description}</small> : null}
                                        </td>
                                        <td style={{ padding: "16px 20px" }}>
                                            <div className="actions">
                                                <button type="button" className="btn" onClick={() => openEdit(chore)}>
                                                    Edit
                                                </button>

                                                {chore.isCritical ? (
                                                    <>
                                                        <button type="button" className="btn" onClick={() => openCritical(chore)}>
                                                            Edit Critical
                                                        </button>
                                                        <button type="button" className="btn" onClick={() => void removeCritical(chore)}>
                                                            Remove Critical
                                                        </button>
                                                    </>
                                                ) : (
                                                    <button type="button" className="btn" onClick={() => openCritical(chore)}>
                                                        Critical
                                                    </button>
                                                )}

                                                <button type="button" className="btn btn-danger" onClick={() => openDelete(chore)}>
                                                    Delete
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
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
            <ModalShell open={modal !== "none"} busy={modalBusy} onRequestClose={closeModal}>
                {renderModalBody()}
            </ModalShell>

            {/* Toast */}
            <div
                className={`toast ${toast ? "visible" : ""}`}
                style={{
                    borderColor: toast?.positive ? "rgba(52, 211, 153, 0.4)" : "rgba(248, 113, 113, 0.4)",
                    color: toast?.positive ? "var(--success)" : "var(--danger)"
                }}
            >
                {toast?.message ?? ""}
            </div>
        </>
    );
}
