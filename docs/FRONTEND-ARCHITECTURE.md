# Bureau Frontend Architecture

This document describes how the Bureau frontend is structured, how it connects to the backend,
how to run it in development, and how to add new admin modules.

---

## 1. High-level picture

Bureau is a **standalone SPA** (Vite + React + React Router) served by Nginx. The .NET solution
owns REST APIs only — no Razor pages, no Web hosts, no tag helpers.

```
┌─────────────────────────────────────────────────────────┐
│  bureau-web (Nginx container)                           │
│  app/client/apps/bureau-web/                            │
│                                                         │
│  React Router                                           │
│  ├── /nodes/*   → @bureau/nodes-admin                   │
│  └── /chores/*  → @bureau/chores-admin                  │
│                                                         │
│  Feature visibility: JWT scopes from auth provider      │
└──────────────┬──────────────────────────────────────────┘
               │  fetch  (Authorization: Bearer <jwt>)
     ┌─────────┴──────────┐
     │                    │
Watson.Nodes.Api     Niles.Chores.Api
:5200                :5300
```

---

## 2. Repository layout

```
bureau/
├── server/                         .NET — APIs only
│   └── src/
│       ├── Bureau.Primitives/          FeatureKeys, FeaturesOptions
│       ├── Bureau.AspNetCore/          RequireFeatureAttribute
│       ├── Bureau.Server.Hosting/      Auth wiring (Dev + OIDC), CORS
│       ├── Bureau.Frontend.DevRunner/  Starts pnpm dev from app/
│       ├── Niles.Chores.Api/           Chores REST API  (:5300)
│       ├── Watson.Nodes.Api/           Nodes REST API   (:5200)
│       └── Watson.Items.Ingest.Api/    Items ingest API (:5210)
│
└── app/                            Frontend — fully standalone
    ├── package.json                pnpm workspace root
    ├── pnpm-workspace.yaml
    ├── client/
    │   ├── apps/
    │   │   ├── bureau-web/         Standalone Vite SPA
    │   │   └── playground/         Component dev sandbox
    │   └── libs/
    │       ├── client-core/        HTTP client, auth, paging types
    │       ├── bureau-shell/       Layout, sidebar, CSS design system
    │       ├── chores-admin/       Chores admin React module
    │       └── nodes-admin/        Nodes admin React module
    ├── shared/
    │   ├── nodes-core/             Shared node types + API client
    │   └── offline-store/
    └── mobile/
        └── apps/bureau-mobile/     React Native (Expo)
```

---

## 3. bureau-web SPA

### 3.1 Config system

Runtime config is a static JSON file loaded at startup — not baked into the bundle.

```
app/client/apps/bureau-web/public/
├── config.template.json  ← committed; uses $VAR placeholders for envsubst
└── config.json           ← gitignored; created locally from template
```

Shape (`AppConfig`):
```ts
{
  environment: "dev" | "test" | "production",
  auth: {
    mode: "dev" | "oidc",
    oidc?: { authority, clientId, redirectUri }
  },
  apis: {
    chores: "http://localhost:5300",
    nodes:  "http://localhost:5200",
    itemsIngest: "http://localhost:5210"
  }
}
```

In containers, `docker-entrypoint.sh` runs `envsubst < config.template.json > config.json`
before Nginx starts. Each deployment sets its own environment variables.

### 3.2 Auth

When `auth.mode === "dev"`:
- No OIDC redirect — auto-logged-in as "dev-user"
- API calls use `Authorization: Bearer dev-token`
- `DevAuthProvider` injects all feature scopes (full access)

When `auth.mode === "oidc"`:
- Uses `oidc-client-ts` for authorization code + PKCE flow to Sven
- Access token from Sven carries `scope` claims for features
- `OidcAuthProvider` (stub — wired when `feature/sven` is merged)

### 3.3 Feature system

Features are **OAuth 2.0 scopes** in the JWT. No feature flags in `config.json`.

```ts
// After login, build a feature tree from scope strings:
const scopes = ["niles.chores", "watson.nodes", "watson.nodes.crud"];
const tree = buildFeatureTree(scopes);
// → { niles: { chores: true }, watson: { nodes: { crud: true } } }

isFeatureEnabled(tree, "watson.nodes.crud") // → true
isFeatureEnabled(tree, "watson.nodes.analytics") // → false
```

Hook:
```ts
const enabled = useFeature(FeatureKeys.Niles.Chores); // true/false
```

Feature boundary (isolates crashes, lazy-loads):
```tsx
<FeatureBoundary feature="niles.chores" fallback={<FeatureUnavailable name="Chores" />}>
    <ChoresAdmin />
</FeatureBoundary>
```

Feature keys (`src/features/featureKeys.ts`) mirror `Bureau.Features.FeatureKeys` (C#).
Keep both in sync manually until a code generation step is added.

### 3.4 bureau-shell lib

Provides the admin shell UI. Migrated from the old `Bureau.Admin.Frame` Razor Class Library.

| File | Purpose |
|------|---------|
| `Layout.tsx` | Sidebar + header + `<Outlet />` |
| `Sidebar.tsx` | Nav items, close button |
| `Header.tsx` | Hamburger, theme toggle, user display |
| `bureau-shell.css` | Full CSS design system (CSS variables: `--bg`, `--surface`, `--ink`, `--muted`, `--line`, `--accent`, `--radius`, ...) |

Light / dark themes via `[data-theme="dark"]` and `prefers-color-scheme`.
Theme choice persisted in `localStorage`.

### 3.5 Bridge pattern

`chores-admin` and `nodes-admin` still call `AppRuntimeOptionsLoader.loadFromWindow()`.
`main.tsx` sets `window.__BUREAU__` from `config.json` before rendering so these modules
keep working without changes:

```ts
window.__BUREAU__ = {
    environment: config.environment,
    version: "1.0.0",
    apiBaseUrls: {
        chores: config.apis.chores,
        nodes:  config.apis.nodes,
        itemsIngest: config.apis.itemsIngest,
    },
};
```

---

## 4. .NET feature system

### 4.1 FeatureKeys (Bureau.Primitives)

```csharp
FeatureKeys.Niles.Chores       // "niles.chores"
FeatureKeys.Watson.Nodes.Crud  // "watson.nodes.crud"
FeatureKeys.AllKeys            // IReadOnlyList<string> — all known keys
```

### 4.2 FeaturesOptions (Bureau.Primitives)

Bound from `appsettings.json` section `"Features"`. Opt-out model (default = enabled).

```json
{
  "Features": {
    "Disabled": ["watson.nodes.analytics"]
  }
}
```

### 4.3 RequireFeatureAttribute (Bureau.AspNetCore)

Guards API endpoints with two checks:

1. **Master switch** — if key is in `Features.Disabled` → 404 (feature doesn't exist)
2. **JWT scope claim** — if token lacks the `scope` claim → 403 (feature exists, user not permitted)

```csharp
[RequireFeature(FeatureKeys.Watson.Nodes.Crud)]
[HttpGet("/nodes")]
public IActionResult GetNodes() { ... }
```

Register in Program.cs:
```csharp
builder.Services.AddBureauFeatures(builder.Configuration);
```

### 4.4 Dev mode — feature scope injection

`DevApiTokenAuthenticationHandler` accepts `Authorization: Bearer dev-token`.
`DevPrincipalFactory` injects `scope` claims from `Auth:Dev:Features` in appsettings:

```json
// appsettings.Development.json
{
  "Auth": {
    "Mode": "Dev",
    "Dev": {
      "UserName": "dev",
      "Roles": ["Admin"],
      "Features": []
    }
  }
}
```

Empty `Features` list → all `FeatureKeys.AllKeys` are injected (full access).
Non-empty list → only listed scopes injected (simulate restricted user).

---

## 5. Running in development

### 5.1 Prerequisites

- .NET 8 SDK
- Node.js (LTS) + pnpm (`corepack enable && corepack prepare pnpm@latest --activate`)
- SQL Server (Docker recommended: `docker compose -f server/deploy/docker-compose.dev.yml up -d`)

### 5.2 Start everything

**Option A — Visual Studio multi-project launch**

Open `server/Bureau.slnx`. Select the `Bureau[Dev]` launch profile from the startup dropdown.
Starts: `Niles.Chores.Api` + `Watson.Nodes.Api` + `Bureau.Frontend.DevRunner`.

DevRunner spawns `pnpm dev` from `app/` — this starts `bureau-web` Vite dev server on port 5000.

**Option B — Separate terminals**

```bash
# Terminal 1: Chores API
dotnet run --project server/src/Niles.Chores.Api
# → http://localhost:5300

# Terminal 2: Nodes API
dotnet run --project server/src/Watson.Nodes.Api
# → http://localhost:5200

# Terminal 3: Frontend
cd app && pnpm install && pnpm dev
# → http://localhost:5000
```

Open `http://localhost:5000`. Auto-logged-in as dev user (no password). Full access to all features.

### 5.3 Simulate restricted access

Edit `server/src/Niles.Chores.Api/appsettings.Development.json`:
```json
{
  "Auth": {
    "Dev": {
      "Features": ["niles.chores"]
    }
  }
}
```

The Nodes section disappears from the sidebar. Nodes API endpoints return 403.

---

## 6. Port assignments

| Range | Domain | Assigned ports |
|-------|--------|---------------|
| `50XX` | Frontend dev servers | `5000` bureau-web, `5010` playground |
| `51XX` | Auth — Sven (TBD) | `5100` Sven |
| `52XX` | Watson domain | `5200` Watson.Nodes.Api, `5210` Watson.Items.Ingest.Api |
| `53XX` | Niles domain | `5300` Niles.Chores.Api |

**Rule for new APIs:** find the domain block, take the next unused `X0` slot.
Document in `docs/DEPLOYMENT.md`.

---

## 7. How to add a new admin section

1. **Create a lib** in `app/client/libs/{name}/`:
   - `package.json` — name `@bureau/{name}`, depends on `@bureau/client-core`
   - `src/index.ts` — exports the root component + `boot()`
   - `src/{Name}AdminApp.tsx` — root component

2. **Register in bureau-web**:
   - Add dependency in `app/client/apps/bureau-web/package.json`
   - Add alias in `vite.config.ts` resolve.alias
   - Add feature key to `FeatureKeys` (both `featureKeys.ts` and `FeatureKeys.cs`)
   - Add route + nav item in `App.tsx` behind `useFeature`

3. **Add feature keys** to `Bureau.Primitives/Features/FeatureKeys.cs` and mirror in
   `app/client/apps/bureau-web/src/features/featureKeys.ts`

4. **Annotate API endpoints** with `[RequireFeature(FeatureKeys.Your.Key)]`

5. **pnpm install** from `app/` — pnpm adds the new workspace dependency

---

## 8. Design principles

1. **Frontend is fully standalone.** The web app starts, runs, and builds without .NET.
   .NET owns only REST APIs.

2. **Config at runtime, not build time.** `config.json` is loaded at startup, not baked in.
   One Docker image works in all environments — only env vars change.

3. **Features are JWT scopes.** The auth server (Sven) is the single source of truth for who
   has access to what. No per-API user-feature tables. No feature flags in config.

4. **Isolation by default.** Each feature module is lazy-loaded and wrapped in a React
   `ErrorBoundary`. A crash in one module does not affect others.

5. **CSS variables from the shell.** All modules use `var(--bg)`, `var(--surface)`,
   `var(--ink)`, etc. from `bureau-shell.css`. Visual consistency without a shared component
   library.

6. **Plain React state.** `useState` + `useEffect`. No Redux, no Zustand.
