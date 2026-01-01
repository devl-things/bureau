// bureau-admin-frame.js
// Vanilla JS only:
// - Sidebar open/close on ALL displays (same behavior)
// - Hamburger only opens (it disappears when open; close is inside sidebar)
// - Backdrop + ESC + close on nav click (mobile)
// - Swipe left on sidebar to close (mobile)
// - Theme toggle (soft light/dark) with localStorage persistence

(function () {
    "use strict";

    var root = document.documentElement;

    function isMobile() {
        return window.matchMedia("(max-width: 1023px)").matches;
    }

    // -----------------------------
    // Sidebar
    // -----------------------------

    var shell = document.querySelector(".admin-shell");
    var sidebar = document.getElementById("adminSidebar");
    var navToggle = document.getElementById("adminNavToggle"); // hamburger (only visible when closed)
    var navClose = document.getElementById("adminNavClose");   // close icon inside sidebar
    var backdrop = document.getElementById("adminBackdrop");

    function setBackdropVisible(visible) {
        if (!backdrop) {
            return;
        }
        backdrop.hidden = !visible;
    }

    function applyOpenEffects() {
        if (!shell) {
            return;
        }

        var open = shell.classList.contains("nav-open");

        if (navToggle) {
            navToggle.setAttribute("aria-expanded", open ? "true" : "false");
        }

        if (isMobile() && open) {
            setBackdropVisible(true);
            document.body.style.overflow = "hidden";
            return;
        }

        setBackdropVisible(false);
        document.body.style.overflow = "";
    }

    function openNav() {
        if (!shell) {
            return;
        }
        shell.classList.add("nav-open");
        applyOpenEffects();
    }

    function closeNav() {
        if (!shell) {
            return;
        }
        shell.classList.remove("nav-open");
        applyOpenEffects();
    }

    // Default state: sidebar hidden everywhere until opened
    if (shell) {
        shell.classList.remove("nav-open");
        applyOpenEffects();
    }

    // Hamburger only opens (once open, it's removed from DOM flow by CSS)
    if (navToggle) {
        navToggle.addEventListener("click", function () {
            openNav();
        });
    }

    // Close from inside sidebar (mobile + desktop)
    if (navClose) {
        navClose.addEventListener("click", function () {
            closeNav();
        });
    }

    // Backdrop closes (mobile only)
    if (backdrop) {
        backdrop.addEventListener("click", function () {
            closeNav();
        });
    }

    // ESC closes
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            closeNav();
        }
    });

    // Close on link click on mobile (keeps UX tight)
    if (sidebar) {
        sidebar.addEventListener("click", function (e) {
            if (!isMobile()) {
                return;
            }

            var target = e.target;
            if (!target) {
                return;
            }

            while (target && target !== sidebar && target.tagName !== "A") {
                target = target.parentElement;
            }

            if (target && target.tagName === "A") {
                closeNav();
            }
        });
    }

    // Keep backdrop/scroll correct when resizing
    window.addEventListener("resize", function () {
        applyOpenEffects();
    });

    // -----------------------------
    // Swipe left to close (mobile)
    // -----------------------------

    (function setupSwipeToClose() {
        if (!sidebar) {
            return;
        }

        var startX = 0;
        var startY = 0;
        var tracking = false;

        var minSwipeX = 70;
        var maxOffAxisY = 60;

        sidebar.addEventListener("touchstart", function (e) {
            if (!isMobile()) {
                return;
            }
            if (!shell || !shell.classList.contains("nav-open")) {
                return;
            }
            if (!e.touches || e.touches.length !== 1) {
                return;
            }

            var t = e.touches[0];
            startX = t.clientX;
            startY = t.clientY;
            tracking = true;
        }, { passive: true });

        sidebar.addEventListener("touchmove", function (e) {
            if (!tracking) {
                return;
            }
            if (!e.touches || e.touches.length !== 1) {
                tracking = false;
                return;
            }

            var t = e.touches[0];
            var dx = t.clientX - startX;
            var dy = t.clientY - startY;

            if (Math.abs(dy) > maxOffAxisY && Math.abs(dy) > Math.abs(dx)) {
                tracking = false;
            }
        }, { passive: true });

        sidebar.addEventListener("touchend", function (e) {
            if (!tracking) {
                return;
            }
            tracking = false;

            if (!e.changedTouches || e.changedTouches.length < 1) {
                return;
            }

            var t = e.changedTouches[0];
            var dx = t.clientX - startX;
            var dy = t.clientY - startY;

            if (dx < -minSwipeX && Math.abs(dy) < maxOffAxisY) {
                closeNav();
            }
        }, { passive: true });
    })();

    // -----------------------------
    // Theme toggle
    // -----------------------------

    var themeToggle = document.getElementById("adminThemeToggle");
    var storageKey = "bureau_admin_theme"; // "light" | "dark" | null (auto)

    function readSavedTheme() {
        try {
            var value = localStorage.getItem(storageKey);
            if (value === "light" || value === "dark") {
                return value;
            }
            return null;
        } catch (e) {
            return null;
        }
    }

    function applyTheme(theme) {
        if (theme === "light" || theme === "dark") {
            root.setAttribute("data-theme", theme);
            try { localStorage.setItem(storageKey, theme); } catch (e) { }
            return;
        }

        root.removeAttribute("data-theme");
        try { localStorage.removeItem(storageKey); } catch (e) { }
    }

    function getExplicitTheme() {
        var value = root.getAttribute("data-theme");
        if (value === "light" || value === "dark") {
            return value;
        }
        return null;
    }

    applyTheme(readSavedTheme());

    if (themeToggle) {
        themeToggle.addEventListener("click", function () {
            var current = getExplicitTheme();
            if (current === "dark") {
                applyTheme("light");
                return;
            }
            if (current === "light") {
                applyTheme(null);
                return;
            }
            applyTheme("dark");
        });
    }
})();
