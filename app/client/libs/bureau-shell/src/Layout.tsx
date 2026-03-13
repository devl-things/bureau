import React, { useCallback, useEffect, useRef, useState } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { Header } from "./Header";
import { Sidebar } from "./Sidebar";
import type { NavItem } from "./types";

interface LayoutProps {
    navItems: NavItem[];
    pageTitle?: string;
    userName?: string;
}

export function Layout({ navItems, pageTitle, userName }: LayoutProps): React.ReactElement {
    const [navOpen, setNavOpen] = useState(false);
    const sidebarRef = useRef<HTMLElement>(null);
    const location = useLocation();

    const open = useCallback(() => setNavOpen(true), []);
    const close = useCallback(() => setNavOpen(false), []);

    // Close sidebar on route change (mobile UX)
    useEffect(() => { close(); }, [location.pathname, close]);

    // ESC key closes sidebar
    useEffect(() => {
        function onKey(e: KeyboardEvent) { if (e.key === "Escape") close(); }
        document.addEventListener("keydown", onKey);
        return () => document.removeEventListener("keydown", onKey);
    }, [close]);

    // Lock body scroll when mobile sidebar is open
    useEffect(() => {
        const isMobile = window.matchMedia("(max-width: 1023px)").matches;
        document.body.style.overflow = navOpen && isMobile ? "hidden" : "";
        return () => { document.body.style.overflow = ""; };
    }, [navOpen]);

    // Swipe left on sidebar to close (mobile)
    useEffect(() => {
        const sidebar = sidebarRef.current;
        if (!sidebar) return;

        let startX = 0;
        let startY = 0;
        let tracking = false;
        const minSwipeX = 70;
        const maxOffAxisY = 60;

        function onTouchStart(e: TouchEvent) {
            if (!window.matchMedia("(max-width: 1023px)").matches) return;
            if (!navOpen || e.touches.length !== 1) return;
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            tracking = true;
        }

        function onTouchMove(e: TouchEvent) {
            if (!tracking || e.touches.length !== 1) { tracking = false; return; }
            const absDy = Math.abs(e.touches[0].clientY - startY);
            const absDx = Math.abs(e.touches[0].clientX - startX);
            if (absDy > maxOffAxisY && absDy > absDx) tracking = false;
        }

        function onTouchEnd(e: TouchEvent) {
            if (!tracking) return;
            tracking = false;
            if (!e.changedTouches.length) return;
            const dx = e.changedTouches[0].clientX - startX;
            const absDy = Math.abs(e.changedTouches[0].clientY - startY);
            if (dx < -minSwipeX && absDy < maxOffAxisY) close();
        }

        sidebar.addEventListener("touchstart", onTouchStart, { passive: true });
        sidebar.addEventListener("touchmove", onTouchMove, { passive: true });
        sidebar.addEventListener("touchend", onTouchEnd, { passive: true });
        return () => {
            sidebar.removeEventListener("touchstart", onTouchStart);
            sidebar.removeEventListener("touchmove", onTouchMove);
            sidebar.removeEventListener("touchend", onTouchEnd);
        };
    }, [navOpen, close]);

    return (
        <div className={`admin-shell${navOpen ? " nav-open" : ""}`}>
            <aside className="admin-sidebar" id="adminSidebar" ref={sidebarRef}>
                <Sidebar navItems={navItems} onClose={close} />
            </aside>

            <div className="admin-main">
                <header className="admin-header">
                    <Header onMenuOpen={open} pageTitle={pageTitle} userName={userName} />
                </header>
                <main className="admin-content">
                    <Outlet />
                </main>
            </div>

            {/* Backdrop — mobile only, closes sidebar on tap */}
            {navOpen && (
                <div
                    className="admin-backdrop"
                    role="presentation"
                    onClick={close}
                />
            )}
        </div>
    );
}
