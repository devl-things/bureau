import React from "react";
import { NavLink } from "react-router-dom";
import type { NavItem } from "./types";

interface SidebarProps {
    navItems: NavItem[];
    onClose: () => void;
}

export function Sidebar({ navItems, onClose }: SidebarProps): React.ReactElement {
    return (
        <>
            <div className="admin-sidebar-top">
                <div className="admin-brand">
                    <div className="admin-nav-title">Bureau</div>
                    <div className="admin-nav-subtitle">Admin</div>
                </div>
                <button
                    type="button"
                    className="admin-nav-close"
                    aria-label="Close menu"
                    onClick={onClose}
                >
                    <svg className="icon icon-close" viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M6 6l12 12M18 6L6 18" />
                    </svg>
                </button>
            </div>

            <nav className="admin-nav">
                <ul className="admin-nav-list">
                    {navItems.map(item => (
                        <li key={item.key} className="admin-nav-item">
                            <NavLink
                                className={({ isActive }) =>
                                    isActive ? "admin-nav-link active" : "admin-nav-link"
                                }
                                to={item.path}
                            >
                                <span className="admin-nav-text">{item.title}</span>
                            </NavLink>
                        </li>
                    ))}
                </ul>
            </nav>
        </>
    );
}
