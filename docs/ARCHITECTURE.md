# 🏛️ [WIP] Bureau – Architecture Overview

This document describes the **naming conventions, project structure, architectural principles, and long-term vision** of the `bureau` monorepo.

It is intentionally opinionated and pragmatic. The goal is **clarity, scalability, and long-term sanity**, not theoretical purity.
This repository follows a layered (onion/clean) architecture composed of Core, Infrastructure, Presentation, and Tests.  
See [Solution Structure](../server/docs/SOLUTION_STRUCTURE.md) for details.
---

## 1. Core Philosophy

* **Monorepo, not monolith**
* Projects live together but **interact like microservices**
* Clear ownership per domain
* Clear dependency direction
* Shared concepts are explicit, not implicit

> If two projects talk to each other, it should be obvious *how* and *why*.

---

## 2. Domain Umbrellas

### 2.1 `Sven` — Identity & Authentication Platform

**Purpose**

* OAuth2 / OpenID Connect Identity Provider
* One user ↔ many external identities (Google, Microsoft, etc.)
* Foundation for future access to external systems (mail, APIs, integrations)

**Characteristics**

* Central issuer of identity and tokens
* Used by all other domains
* `Sven.Contracts` is expected to be widely consumed
* Service-to-service auth may be added later (not prematurely)

---

### 2.2 `Watson` — Shared Semantic / Knowledge Layer

**Purpose**

* Canonical definitions
* Cross-domain semantics
* Linking glue between domains

**Watson owns**

* Items (what things *are*)
* Tags (semantic connections)
* Scenarios (e.g. “Guests visiting”)

**Watson does NOT**

* Orchestrate workflows
* Contain business behavior
* Know about chores/tasks logic

> Watson answers: *“What exists, and how things relate.”*

---

### 2.3 `Niles` — Household Operations

**Purpose**

* Home-related execution and maintenance

**Examples**

* `Niles.Chores` — cleaning, maintenance
* `Niles.ShoppingLists` — planning purchases

Niles **uses** Watson concepts (`ItemId`, `TagId`) but does not own them.

---

### 2.4 `Moneypenny` — Personal Assistant / Tasks

**Purpose**

* Generic, non-household tasks
* Personal planning and reminders

**Examples**

* dentist appointment
* collecting items
* preparing documents

Uses Watson for shared meaning and Sven for identity.

---

## 3. Naming & Dependency Rules

### 3.1 Naming encodes dependency

**Rule**

> If a project is named `X.Y.Z`, it depends on `X.Y`.

**Exception**

* `*.Contracts` projects depend on nothing inward

Examples:

* `Watson.Items.Api` → depends on `Watson.Items`
* `Watson.Items.Ingest.Excel` → depends on `Watson.Items`
* `Watson.Items.Contracts` → depends on no Watson implementation

---

## 4. Canonical Project Structure (Example: Watson.Items)

### 4.1 `Watson.Items`

**The domain implementation**

Contains:

* Domain models
* Services / application logic
* Internal interfaces
* Persistence (DB, EF, migrations — kept here initially for simplicity)

This is the **center of the onion**.

---

### 4.2 `Watson.Items.Api`

**HTTP entrypoint (transport layer)**

Contains:

* Controllers / endpoints
* Authentication & authorization (when needed)
* Mapping HTTP → domain services

Depends on:

* `Watson.Items`

Supports:

* Public endpoints (`/api/*`)
* Internal endpoints (`/internal/*`)

Exposure is controlled at nginx / ingress level.

---

### 4.3 `Watson.Items.Ingest.Excel`

**Ingestion entrypoint (internal infrastructure)**

Contains:

* Excel / external source readers
* Data transformation logic
* Calls domain services directly

Rules:

* Does NOT use HTTP
* Does NOT write to DB directly
* Reuses domain validation & normalization

Depends on:

* `Watson.Items`

Ingest is treated as **infrastructure / outer ring**, not a separate domain.

---

### 4.4 `Watson.Items.Contracts` (introduced only when needed)

**Shared contracts for other domains**

Contains:

* IDs (`ItemId`, `TagId`)
* Shared DTOs / read models
* Enums / value objects

Rules:

* Does NOT depend on `Watson.Items`
* `Watson.Items` does NOT depend on it
* API maps between domain ↔ contracts

Created only when another domain (Niles/Moneypenny) truly consumes Items.

---

## 5. Abstractions vs Contracts (Important Distinction)

### Abstractions

* Internal interfaces
* Replaceable implementations
* Used for testing or future flexibility

**Live inside the domain project** unless there is a strong reason to extract.

### Contracts

* Shared types for external consumers
* Compile-time boundary between domains

**Never mix these two concepts.**

---

## 6. Authentication & Exposure Strategy

### Current state

* All services are internal
* Network-level isolation is sufficient
* No premature auth between services

### Rule

> If an API is reachable from the browser or internet → authentication is mandatory.

### Patterns

* **Internal APIs** → docker-network only
* **Public APIs** → exposed via nginx + Sven
* **Admin UIs** → prefer BFF pattern

---

## 7. Public vs Internal Endpoints (Same Project)

Allowed and recommended:

* `/api/*` → public, user-authenticated
* `/internal/*` → internal, network-restricted

Same project, different policies, different exposure rules.

---

## 8. Clients & Service Consumption (Planned Pattern)

Future direction:

* `*.Client` projects (typed HTTP clients)
* Wrap HTTP, auth, retries

Examples:

* `Watson.Items.Client`
* `Sven.Client`

Service-to-service auth via Sven will be added **when needed**, not earlier.

---

## 9. Guiding Principles (Summary)

* Pragmatic over dogmatic
* Onion architecture where it matters
* Microservice boundaries by convention
* Naming must encode intent
* Introduce complexity only when there is a consumer
* Internal ≠ public
* Monorepo ≠ free-for-all

---

**This document is the reference point for all future projects inside `bureau`.**
