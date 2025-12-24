# Frontend Workspace Structure (`client/`)

This document describes how frontend code is organized in this repository and how it is intended to be used.

---

## Purpose

The `client/` workspace contains **all frontend source code** that is not implemented directly as Razor or server-rendered UI inside ASP.NET projects.

It is designed to:

* support **shared UI and logic** across multiple admin applications
* allow **independent small ASP.NET hosts** to consume frontend bundles
* keep frontend concerns clearly separated from backend (`server/`) code
* scale gradually without forcing a single mega-SPA

---

## High-level structure

```
client/
├─ apps/
│  └─ bureau-admin/
│     ├─ src/
│     ├─ entries/
│     └─ package.json
│
├─ packages/
│  ├─ admin-core/
│  └─ admin-ui/
│
└─ README.md
```

---

## `apps/`

The `apps/` directory contains **runnable frontend applications**.

An app:

* has its own `package.json`
* can be built and/or run independently
* produces one or more **frontend bundles** (JS/CSS)

### `apps/bureau-admin`

This app produces **React entry bundles** used by admin features.

Key characteristics:

* **MPA-style**: multiple entry points (e.g. chores admin, items admin)
* bundles are **mounted inside ASP.NET host pages**, not run as a standalone SPA
* routing is handled by the host (server-side), not React Router

Example entries:

* `entries/chores-admin.tsx`
* `entries/items-admin.tsx`

The app itself does **not** own authentication or navigation; those are handled by the ASP.NET host and the shared frame.

---

## `packages/`

The `packages/` directory contains **shared frontend libraries** that are reused across apps.

Packages:

* are **not runnable by themselves**
* do not define pages or routes
* are consumed by one or more apps

### `packages/admin-core`

**No React dependency.**

Responsibilities:

* loading browser-facing config (e.g. from `/config`)
* API client helpers (`fetch` wrapper)
* auth header handling (dev token now, Sven/OIDC later)
* ProblemDetails-style error parsing

This package is intentionally framework-agnostic so it can be reused by:

* React-based admin features
* potential future non-React frontend tooling

---

### `packages/admin-ui`

**React-only UI components.**

Responsibilities:

* reusable admin UI building blocks
* consistent look & behavior across admin features

Examples:

* tables & pagination
* dialogs & confirmations
* toast/notification system
* search inputs

Packages in `admin-ui` must **not** contain API logic or business rules.

---

## Relationship to ASP.NET hosts

ASP.NET hosts (e.g. `Bureau.Admin.Host`, `Niles.Chores.Web`) are responsible for:

* routing
* authentication
* rendering the shared admin frame
* mounting frontend bundles produced by `client/apps/*`

Frontend code in `client/`:

* never renders the global frame
* never owns navigation between admin applications
* is always mounted into a host-provided page

---

## Design rules

1. **Backend code stays in `server/`**
2. **Frontend code stays in `client/`**
3. `apps/` produce bundles; `packages/` provide reusable code
4. Hosts decide *where* and *when* a frontend bundle is rendered
5. Auth and navigation are host responsibilities, not frontend responsibilities

---

## Evolution

This structure allows gradual evolution:

* existing static or Razor-based admin pages can coexist with React-based features
* shared UI and logic can be extracted incrementally into packages
* no forced migration to a single SPA or microfrontend architecture

---

## Summary

The `client/` workspace exists to provide **shared, reusable frontend foundations** while keeping:

* admin hosts small and independent
* navigation and auth centralized in .NET
* frontend complexity contained and optional
