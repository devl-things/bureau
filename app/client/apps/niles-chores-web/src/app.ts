import { ApiClient } from "@bureau/client-core";
import type { AppRuntimeOptions } from "@bureau/client-core";
import { ChoresApi } from "@bureau/chores-core";
import type { PrioritizedChoreDto } from "@bureau/chores-core";

// ---------------------------------------------------------------------------
// Config
// ---------------------------------------------------------------------------

type ChoresConfig = {
    environment: string;
    apis: { chores: string };
};

async function loadConfig(): Promise<ChoresConfig> {
    const res = await fetch("/config.json");
    if (!res.ok) throw new Error("Failed to load config.json");
    return res.json() as Promise<ChoresConfig>;
}

// ---------------------------------------------------------------------------
// Priority constants
// ---------------------------------------------------------------------------

const PRIORITY = { CRITICAL: 1, HIGH: 2, MEDIUM: 3, LOW: 4 } as const;

function priorityNameFromNumber(n: number): string {
    switch (n) {
        case PRIORITY.CRITICAL: return "critical";
        case PRIORITY.HIGH: return "high";
        case PRIORITY.LOW: return "low";
        default: return "medium";
    }
}

function priorityLabelShort(name: string): string {
    switch (name) {
        case "critical": return "C";
        case "high": return "H";
        case "medium": return "M";
        case "low": return "L";
        default: return "?";
    }
}

function priorityTooltip(name: string): string {
    return `${name.charAt(0).toUpperCase()}${name.slice(1)} priority`;
}

function normalizeType(value: string | null | undefined): string {
    if (!value) return "Maintenance";
    return value.toLowerCase() === "extra" ? "Extra" : "Maintenance";
}

// ---------------------------------------------------------------------------
// Chore view model
// ---------------------------------------------------------------------------

type ChoreViewModel = {
    id: string;
    title: string;
    description: string;
    completed: boolean;
    criticalityNumber: number;
    criticalityName: string;
    priority: number;
    type: string;
    note: string;
};

function toViewModel(item: PrioritizedChoreDto, index: number): ChoreViewModel {
    const criticalityNumber = Number(item.criticality) || PRIORITY.MEDIUM;
    return {
        id: item.id ?? String(index),
        title: item.title ?? "Untitled chore",
        description: item.description ?? "",
        completed: !!item.completed,
        criticalityNumber,
        criticalityName: priorityNameFromNumber(criticalityNumber),
        priority: Number(item.priority) || 0,
        type: normalizeType(item.type),
        note: (item.note ?? "").trim(),
    };
}

// ---------------------------------------------------------------------------
// DOM helpers
// ---------------------------------------------------------------------------

function el<T extends HTMLElement>(id: string): T {
    return document.getElementById(id) as T;
}

let todayIso = "";

function renderTodayDate(): void {
    const now = new Date();
    el("today-date").textContent = now.toLocaleDateString(undefined, {
        weekday: "long", year: "numeric", month: "long", day: "numeric",
    });
    todayIso = now.toISOString().slice(0, 10);
}

function setStatus(message: string, mode: "online" | "offline"): void {
    el("status-message").textContent = message;
    const label = el<HTMLSpanElement>("status-label");
    const indicator = el("status-pill").querySelector(".status-indicator")!;
    if (mode === "offline") {
        label.textContent = "Offline";
        label.classList.add("offline");
        indicator.classList.add("status-dot-offline");
    } else {
        label.textContent = "Online";
        label.classList.remove("offline");
        indicator.classList.remove("status-dot-offline");
    }
}

function setLoading(isLoading: boolean): void {
    el("loading-row").style.display = isLoading ? "flex" : "none";
    const btn = el<HTMLButtonElement>("refresh-btn");
    btn.disabled = isLoading;
    btn.classList.toggle("loading", isLoading);
}

function updateProgress(done: number, total: number): void {
    const percent = total === 0 ? 0 : Math.round((done / total) * 100);
    el("progress-fill").style.width = `${percent}%`;
}

function updateCount(done: number, total: number): void {
    el("chores-count").textContent = `${done} done · ${total} total`;
}

function clearChoresList(): void {
    el("chores-list").innerHTML = "";
    el("empty-state").style.display = "block";
    updateProgress(0, 0);
    updateCount(0, 0);
}

function getCompletedIds(): string[] {
    return Array.from(document.querySelectorAll<HTMLInputElement>('input[name="chore"]'))
        .filter(cb => cb.checked)
        .map(cb => cb.value);
}

function refreshProgressAndCount(): void {
    const checkboxes = Array.from(document.querySelectorAll<HTMLInputElement>('input[name="chore"]'));
    const total = checkboxes.length;
    const done = checkboxes.filter(cb => cb.checked).length;
    updateProgress(done, total);
    updateCount(done, total);
}

function reorderList(): void {
    const listEl = el("chores-list");
    const items = Array.from(listEl.children) as HTMLLIElement[];
    items.sort((a, b) => {
        const aChecked = (a.querySelector<HTMLInputElement>('input[name="chore"]')?.checked) ?? false;
        const bChecked = (b.querySelector<HTMLInputElement>('input[name="chore"]')?.checked) ?? false;
        if (aChecked !== bChecked) return aChecked ? 1 : -1;
        const aCrit = Number(a.dataset.criticalityNumber ?? PRIORITY.MEDIUM);
        const bCrit = Number(b.dataset.criticalityNumber ?? PRIORITY.MEDIUM);
        if (aCrit !== bCrit) return aCrit - bCrit;
        const aPri = Number(a.dataset.priority ?? 0);
        const bPri = Number(b.dataset.priority ?? 0);
        if (aPri !== bPri) return aPri - bPri;
        return Number(a.dataset.originalIndex ?? 0) - Number(b.dataset.originalIndex ?? 0);
    });
    items.forEach(item => listEl.appendChild(item));
}

function handleCheckboxChange(checkbox: HTMLInputElement, li: HTMLLIElement): void {
    li.classList.toggle("completed", checkbox.checked);
    refreshProgressAndCount();
    reorderList();
}

function openNoteModal(title: string, note: string): void {
    el("note-modal-chore-title").textContent = title;
    el("note-modal-content").textContent = note;
    el("note-modal").classList.add("visible");
}

function closeNoteModal(): void {
    el("note-modal").classList.remove("visible");
}

// ---------------------------------------------------------------------------
// Render chores
// ---------------------------------------------------------------------------

function renderChores(chores: ChoreViewModel[]): void {
    const listEl = el("chores-list");
    listEl.innerHTML = "";

    if (!chores.length) {
        el("empty-state").style.display = "block";
        updateProgress(0, 0);
        updateCount(0, 0);
        return;
    }

    el("empty-state").style.display = "none";

    chores.forEach((chore, index) => {
        const li = document.createElement("li");
        li.className = "chore";
        li.dataset.choreId = chore.id;
        li.dataset.criticalityNumber = String(chore.criticalityNumber);
        li.dataset.priority = String(chore.priority);
        li.dataset.originalIndex = String(index);

        const leftDiv = document.createElement("div");
        leftDiv.className = "chore-left";

        const textsDiv = document.createElement("div");
        textsDiv.className = "chore-texts";

        const titleEl = document.createElement("div");
        titleEl.className = "chore-title";
        titleEl.textContent = chore.title;
        textsDiv.appendChild(titleEl);

        if (chore.description) {
            const descEl = document.createElement("div");
            descEl.className = "chore-desc";
            descEl.textContent = chore.description;
            textsDiv.appendChild(descEl);
        }

        const metaEl = document.createElement("div");
        metaEl.className = "chore-meta";

        const priorityPill = document.createElement("div");
        priorityPill.className = `priority-pill priority-${chore.criticalityName}`;
        priorityPill.title = priorityTooltip(chore.criticalityName);
        priorityPill.innerHTML = `<span class="priority-arrow">▲</span><span class="priority-label-short">${priorityLabelShort(chore.criticalityName)}</span>`;

        const typePill = document.createElement("div");
        typePill.className = `type-pill${chore.type === "Extra" ? " extra" : ""}`;
        typePill.textContent = chore.type;

        metaEl.appendChild(priorityPill);
        metaEl.appendChild(typePill);
        textsDiv.appendChild(metaEl);

        if (chore.criticalityNumber === PRIORITY.CRITICAL && chore.note) {
            const noteEl = document.createElement("div");
            noteEl.className = "chore-note";
            noteEl.textContent = chore.note;
            noteEl.addEventListener("click", e => {
                e.stopPropagation();
                if (noteEl.classList.contains("expandable")) openNoteModal(chore.title, chore.note);
            });
            textsDiv.appendChild(noteEl);
            requestAnimationFrame(() => {
                if (noteEl.scrollHeight > noteEl.clientHeight + 1) {
                    noteEl.classList.add("expandable");
                    noteEl.title = "Click to view full note";
                }
            });
        }

        leftDiv.appendChild(textsDiv);

        const checkboxWrap = document.createElement("div");
        checkboxWrap.className = "checkbox-wrap";
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.name = "chore";
        checkbox.value = chore.id;
        checkbox.checked = chore.completed;
        checkboxWrap.appendChild(checkbox);

        li.appendChild(leftDiv);
        li.appendChild(checkboxWrap);

        if (checkbox.checked) li.classList.add("completed");

        li.addEventListener("click", e => {
            if (e.target === checkbox) return;
            checkbox.checked = !checkbox.checked;
            handleCheckboxChange(checkbox, li);
        });
        checkbox.addEventListener("change", () => handleCheckboxChange(checkbox, li));

        listEl.appendChild(li);
    });

    reorderList();
    refreshProgressAndCount();
}

// ---------------------------------------------------------------------------
// Toast
// ---------------------------------------------------------------------------

let toastTimer: ReturnType<typeof setTimeout> | undefined;

function showToast(message: string, isError = false): void {
    const toast = el("toast");
    el("toast-text").textContent = message;
    el("toast-icon").textContent = isError ? "⚠️" : "✨";
    toast.classList.toggle("toast-error", isError);
    toast.classList.add("visible");
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove("visible"), 2600);
}

// ---------------------------------------------------------------------------
// Submit modal
// ---------------------------------------------------------------------------

function openSummaryModal(): void {
    if (!getCompletedIds().length) { showToast("No completed chores to submit.", true); return; }
    el<HTMLInputElement>("modal-hours").value = "";
    el<HTMLInputElement>("modal-minutes").value = "";
    el<HTMLTextAreaElement>("modal-note").value = "";
    el("summary-modal").classList.add("visible");
    el<HTMLInputElement>("modal-hours").focus();
}

function closeSummaryModal(): void {
    el<HTMLButtonElement>("modal-confirm-btn").disabled = false;
    el("summary-modal").classList.remove("visible");
}

// ---------------------------------------------------------------------------
// Main app
// ---------------------------------------------------------------------------

async function checkAndLoad(api: ChoresApi): Promise<void> {
    setLoading(true);
    try {
        setStatus("Checking connection…", "online");
        const healthy = await api.checkHealth();
        if (!healthy) {
            setStatus("API unreachable", "offline");
            el("offline-badge").style.display = "inline";
            clearChoresList();
            showToast("Service currently unavailable.", true);
            return;
        }
        setStatus("Loading chores…", "online");
        el("offline-badge").style.display = "none";
        const chores = await api.getPrioritizedChores(todayIso);
        renderChores(chores.map(toViewModel));
        setStatus("Chores loaded", "online");
    } catch (err) {
        console.error(err);
        setStatus("API unreachable", "offline");
        el("offline-badge").style.display = "inline";
        clearChoresList();
        showToast("Service currently unavailable.", true);
    } finally {
        setLoading(false);
    }
}

async function handleConfirmSubmit(api: ChoresApi): Promise<void> {
    const confirmBtn = el<HTMLButtonElement>("modal-confirm-btn");
    const completedIds = getCompletedIds();
    if (!completedIds.length) { showToast("No completed chores to submit.", true); return; }

    const hours = Math.max(0, Number(el<HTMLInputElement>("modal-hours").value || "0"));
    const minutes = Math.max(0, Number(el<HTMLInputElement>("modal-minutes").value || "0"));
    if (hours * 60 + minutes <= 0) { showToast("Please enter a duration greater than 0.", true); return; }

    const note = el<HTMLTextAreaElement>("modal-note").value.trim();
    confirmBtn.disabled = true;

    try {
        const hh = String(Math.floor((hours * 60 + minutes) / 60)).padStart(2, "0");
        const mm = String((hours * 60 + minutes) % 60).padStart(2, "0");
        await api.submitHousekeeping({
            datetime: new Date().toISOString(),
            duration: `${hh}:${mm}`,
            note: note || null,
            completedChoreIds: completedIds,
        });
        showToast("Chores submitted ✔");
        closeSummaryModal();
        await checkAndLoad(api);
    } catch (err) {
        console.error(err);
        showToast("Couldn't submit chores.", true);
        confirmBtn.disabled = false;
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    const config = await loadConfig();

    const runtime: AppRuntimeOptions = {
        environment: config.environment,
        version: "1.0.0",
        defaultApiKey: "chores",
        apiBaseUrls: { chores: config.apis.chores },
    };
    const apiClient = new ApiClient(runtime);
    const api = new ChoresApi(apiClient, config.apis.chores);

    renderTodayDate();

    el("refresh-btn").addEventListener("click", () => checkAndLoad(api));
    el("submit-btn").addEventListener("click", openSummaryModal);
    el("modal-cancel-btn").addEventListener("click", closeSummaryModal);
    el("modal-confirm-btn").addEventListener("click", () => void handleConfirmSubmit(api));
    el("summary-modal").addEventListener("click", e => { if (e.target === el("summary-modal")) closeSummaryModal(); });
    el("note-modal-close-btn").addEventListener("click", closeNoteModal);
    el("note-modal").addEventListener("click", e => { if (e.target === el("note-modal")) closeNoteModal(); });

    await checkAndLoad(api);
});
