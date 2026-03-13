import { useEffect, useState } from "react";

export type Theme = "dark" | "light" | null; // null = follow OS preference

const STORAGE_KEY = "bureau_admin_theme";

function readSaved(): Theme {
    try {
        const v = localStorage.getItem(STORAGE_KEY);
        if (v === "dark" || v === "light") return v;
    } catch { /* storage unavailable */ }
    return null;
}

export function useTheme() {
    const [theme, setTheme] = useState<Theme>(readSaved);

    useEffect(() => {
        const root = document.documentElement;
        if (theme === "dark" || theme === "light") {
            root.dataset.theme = theme;
            try { localStorage.setItem(STORAGE_KEY, theme); } catch { /* ignore */ }
        } else {
            delete root.dataset.theme;
            try { localStorage.removeItem(STORAGE_KEY); } catch { /* ignore */ }
        }
    }, [theme]);

    // Cycle: auto → dark → light → auto
    function cycleTheme() {
        setTheme(t => t === null ? "dark" : t === "dark" ? "light" : null);
    }

    return { theme, cycleTheme };
}
