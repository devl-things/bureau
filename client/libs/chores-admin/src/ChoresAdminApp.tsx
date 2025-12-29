import React, { useMemo, useState } from "react";
import { AppRuntimeOptionsLoader } from "@bureau/client-core";
import { createAdminApi } from "./api/createAdminClient";
import { ChorePage } from "./pages/ChorePage";
import { HousekeepingPage } from "./pages/HousekeepingPage";

type TabKey = "chores" | "housekeeping";

export function ChoresAdminApp(): React.ReactElement {
    const { api, runtime } = useMemo(() => createAdminApi(), []);
    const [tab, setTab] = useState<TabKey>("chores");

    // runtime in header (env/version) – if createAdminApi already validated, this is safe.
    const headerRuntime = useMemo(() => {
        // In case you ever render app without loader for some reason, keep it robust.
        return runtime ?? AppRuntimeOptionsLoader.loadFromWindow();
    }, [runtime]);

    return (
        <div className="shell">
            <header>
                <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
                    <div>
                        <div className="title">Chores admin</div>
                        <div className="subtitle">Manage chores and housekeeping logs.</div>
                    </div>

                    <div style={{ color: "var(--muted)", fontSize: "0.9rem", alignSelf: "flex-end" }}>
                        env: {headerRuntime.environment} • version: {headerRuntime.version}
                    </div>
                </div>

                <div className="breadcrumbs" style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                    <button
                        type="button"
                        className={`btn ghost`}
                        style={{
                            borderColor: tab === "chores" ? "rgba(56, 189, 248, 0.55)" : undefined
                        }}
                        onClick={() => setTab("chores")}
                    >
                        Chores
                    </button>
                    <button
                        type="button"
                        className={`btn ghost`}
                        style={{
                            borderColor: tab === "housekeeping" ? "rgba(56, 189, 248, 0.55)" : undefined
                        }}
                        onClick={() => setTab("housekeeping")}
                    >
                        Housekeeping
                    </button>
                </div>
            </header>

            {tab === "chores" ? <ChorePage api={api} runtime={runtime} /> : null}
            {tab === "housekeeping" ? <HousekeepingPage api={api} runtime={runtime} /> : null}
        </div>
    );
}
