# Frontend Workspace Structure (`client/`)

This document describes how frontend code is organized in this repository and how it is intended to be used. It represents the **current source of truth** for the Bureau frontend architecture, combining workspace structure, package responsibilities, and the agreed pnpm + Vite bundling strategy.

---

## Purpose

The `client/` workspace contains **all frontend source code** that is not implemented directly as Razor or server-rendered UI inside ASP.NET projects.

It is designed to:

* support **shared UI and logic** across multiple admin and non-admin applications
* allow **independent small ASP.NET hosts** to consume frontend bundles
* keep frontend concerns clearly separated from backend (`server/`) code
* scale gradually without forcing a single mega-SPA
* provide a clean foundation for **React MPAs with shared packages**

---

## Core principles

1. **Clear separation of concerns**

   * Libraries provide reusable logic
   * Apps produce deployable bundles
   * ASP.NET hosts control routing, auth, and layout

2. **Libraries are never served directly**

   * Shared code lives in packages
   * Only built *apps* are served by ASP.NET hosts

3. **Auth and API access are cross-cutting concerns**

   * Admin and non-admin apps both call APIs
   * Auth helpers therefore live in shared packages, not UI-only code

4. **Bundling is a client concern; hosting is a server concern**

   * Vite builds JS/CSS bundles
   * ASP.NET hosts decide which bundles to serve and where to mount them

5. **Frontend modules must be runnable without a host during development**

   * A dedicated playground enables frontend work without wiring into product hosts

---

## High-level structure

```
client/
├─ apps/
│  ├─ playground/
│  └─ bureau-bundles/
│
├─ packages/
│  ├─ client-core/
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

### `apps/bureau-bundles`

This app is a **bundle producer**, not a host or portal.

Responsibilities:

* build React-based frontend bundles using Vite
* support **MPA-style multiple entry points**
* output deployable JS/CSS artifacts

Characteristics:

* bundles are **mounted inside ASP.NET host pages**
* routing is handled by the host (server-side), not React Router
* authentication and navigation are not owned by this app

Example entries:

* `src/entries/chores-admin.tsx`
* `src/entries/items-admin.tsx`
* `src/entries/etl-jobs-admin.tsx`

The app itself does **not** know which host will serve the bundle.

---

### `apps/playground`

The playground is a **frontend-only development environment**.

Purpose:

* develop and test shared packages (`client-core`, `admin-ui`, etc.)
* prototype admin CRUD UIs quickly
* debug API, auth, and error-handling logic

Key characteristics:

* runs via Vite dev server
* proxies API calls to a running backend
* requires **no ASP.NET host wiring**

This satisfies the requirement that frontend modules can be developed and tested independently.

---

## `packages/`

The `packages/` directory contains **shared frontend libraries** reused across apps.

Packages:

* are **not runnable by themselves**
* do not define pages or routes
* are consumed by one or more apps

---

### `packages/client-core`

**No React dependency.**

This is the **shared browser foundation** for all frontend code.

Responsibilities:

* API client helpers (`fetch` wrapper)
* JSON parsing and error handling
* ProblemDetails-style error parsing
* **Auth foundations for calling APIs**

  * abstractions required to later obtain / attach access tokens (e.g. token provider interfaces, auth context primitives)
  * support for adding `Authorization` headers consistently
  * future-ready shape for OIDC/Sven access-token acquisition (implementation may live in separate packages, but the contracts belong here)
* small browser utilities (sanitization wrappers, helpers)

Key rules:

* contains no product-specific endpoint mappings
* contains no UI components
* can be used by admin and non-admin apps

This package defines the *lowest-level frontend contracts*.

---

### `packages/admin-core`

**No React dependency.**

This is a **thin admin-specific layer** on top of `client-core`.

Responsibilities:

* admin-specific auth defaults or conventions
* wiring for dev-token auth in admin scenarios
* future OIDC/Sven admin helpers

Non-goals:

* no API logic duplication
* no UI components
* no hosting knowledge

`admin-core` always depends on `client-core`, never the other way around.

---

### `packages/admin-ui`

**React-only UI components.**

Responsibilities:

* reusable UI building blocks intended primarily for admin experiences
* feature contracts and composition helpers for admin features

Important note:

* **Generic UI primitives** such as tables, pagination, search inputs, dialogs, and notifications can be useful in **both admin and non-admin apps**. When a component is truly generic, it should live in a more general UI package (to be introduced later, e.g. `packages/ui`), and `admin-ui` should contain only admin-flavored composition and conventions.

Rules:

* must not contain API logic
* must not contain business rules
* focuses purely on UI and composition

---

## Relationship to ASP.NET hosts

ASP.NET **hosts** are runnable applications (Visual Studio F5 / deployed services). Examples:

* `Niles.Chores.Web` (host)
* `Niles.Chores.Api` (host)
* `Bureau.Admin` (host)

Separately, there may be shared **libraries** (not hosts) that provide common hosting or UI wiring. For example, a project named like `Bureau.Admin.Hosting` may be a reusable library rather than a runnable host.

Hosts are responsible for:

* routing
* authentication
* rendering the shared **Admin.Frame** (or equivalent shell)
* injecting runtime config
* mounting frontend bundles produced by `client/apps/*`

Frontend code in `client/`:

* never renders the global admin frame
* never owns navigation between applications
* is always mounted into a host-provided page

---

## Bureau Admin Portal (server-side context)

Optionally, a **Bureau Admin Portal** may exist as an ASP.NET host.

Its role:

* act as a directory / launcher for product admin hosts
* use an app registry to list available admin systems
* redirect users to independent admin hosts

It does **not** bundle frontend UI itself.

---

## Build & deployment model

### Development

* Backend: run via Visual Studio (F5)
* Frontend:

  * playground → fast iteration
  * bureau-bundles → real admin feature bundles

### Build

* packages are built as libraries
* apps are bundled via Vite

### Deployment

* ASP.NET hosts copy **only app build output** into `wwwroot`
* shared package artifacts are never served directly

---

## Design rules (summary)

1. Backend code stays in `server/`
2. Frontend code stays in `client/`
3. `apps/` produce bundles; `packages/` provide reusable code
4. Hosts decide *where* and *when* a frontend bundle is rendered
5. Auth and navigation are host responsibilities, with shared primitives in `client-core`

---

## Evolution

This structure allows gradual evolution:

* static or Razor-based admin pages can coexist with React-based features
* shared UI and logic can be extracted incrementally into packages
* no forced migration to a single SPA or microfrontend architecture

---

## Summary

The `client/` workspace provides a **clean, scalable frontend foundation** that supports:

* independent admin hosts
* shared React-based admin features
* gradual modernization
* future OIDC integration

This document should be kept in sync as the frontend architecture evolves.
