import React from "react";
import { useTheme } from "./useTheme";

interface HeaderProps {
    onMenuOpen: () => void;
    pageTitle?: string;
    userName?: string;
}

export function Header({ onMenuOpen, pageTitle, userName }: HeaderProps): React.ReactElement {
    const { cycleTheme } = useTheme();

    return (
        <>
            <button
                type="button"
                className="admin-nav-toggle"
                aria-label="Open menu"
                onClick={onMenuOpen}
            >
                <svg className="icon icon-menu" viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M5 7h14M5 12h14M5 17h14" />
                </svg>
            </button>

            <div className="admin-header-bar">
                <div>
                    {pageTitle && <h1 className="admin-page-title">{pageTitle}</h1>}
                </div>

                <div className="admin-header-right">
                    <button
                        type="button"
                        className="admin-theme-toggle"
                        aria-label="Toggle theme"
                        onClick={cycleTheme}
                    >
                        <svg className="icon icon-theme" viewBox="0 0 24 24" aria-hidden="true">
                            <path
                                className="icon-moon"
                                d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z"
                            />
                            <circle className="icon-sun" cx="12" cy="12" r="4" />
                            <path
                                className="icon-sun"
                                d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"
                            />
                        </svg>
                    </button>

                    {userName
                        ? <span className="admin-user">{userName}</span>
                        : <span className="admin-badge">DEV</span>
                    }
                </div>
            </div>
        </>
    );
}
