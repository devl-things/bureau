# Bureau — Vision

Bureau is a **home-server umbrella application** — a single, self-hosted web interface for
managing tasks, media, notes, and integrations running on a Proxmox home server.

---

## Goals

1. **Single front door.** One URL (`bureau.home`) for all home-server admin: tasks, Nodes,
   item tracking, media (Immich), documents (paperless-ngx), etc.

2. **Own the data.** All data lives on the home server. No cloud dependencies for core features.

3. **Feature-based access control.** Household members see only the features they have access to.
   Permissions are managed per-user in the auth server (Sven) — no code changes needed to
   grant/revoke access.

4. **Modular growth.** New integrations are added as feature modules behind a feature scope.
   A broken module doesn't affect the rest of the app.

---

## Current capabilities

| Feature | Status | Scope |
|---------|--------|-------|
| Nodes admin | Active | `watson.nodes`, `watson.nodes.crud` |
| Chores admin | Active | `niles.chores` |
| Items ingest | In progress | `watson.items` |

---

## Planned integrations

| Integration | What it adds |
|-------------|-------------|
| **paperless-ngx** | Document inbox: auto-tag, search, and act on scanned documents |
| **Immich** | Photo/video library: search, faces, albums, shared links |
| **Home Assistant** | Automation dashboard, sensor history |
| **Jellyfin** | Media requests, playback history |

Integrations appear as feature modules in the bureau-web sidebar behind their own feature scopes.
Users without the relevant scope don't see the feature at all.

---

## Architecture pillars

- **Self-hosted OIDC** (Sven) — users authenticate once, tokens carry feature scopes
- **Standalone SPA** (bureau-web) — React + Vite, served by Nginx, no server-side rendering
- **API-only .NET backend** — no Razor pages, no session state, stateless JWT auth
- **Feature isolation** — each section is a React module in an ErrorBoundary; crashes are contained

---

## Non-goals

- Cloud sync or SaaS hosting
- Public-facing APIs or multi-tenancy
- Mobile-first (mobile app exists but is secondary to web)
