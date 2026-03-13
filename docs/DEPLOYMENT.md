# Bureau Deployment

---

## 1. Environments

| Environment | Auth mode | Frontend | Purpose |
|-------------|-----------|----------|---------|
| **Dev** | `dev` (no OIDC) | Vite dev server (:5000) | Local development |
| **Test** | `dev` (no OIDC) | bureau-web Nginx container | Integration testing |
| **Prod** | `oidc` (Sven) | bureau-web Nginx container | Proxmox home server |

---

## 2. Dev environment

### Prerequisites

- .NET 8 SDK
- Node.js LTS + pnpm
- Docker (for SQL Server)

### One-time setup

```bash
# 1. Start SQL Server
docker compose -f server/deploy/docker-compose.dev.yml up -d

# 2. Copy frontend configs (each app has its own config.json, gitignored)
cp app/client/apps/bureau-web/public/config.template.json \
   app/client/apps/bureau-web/public/config.json

cp app/client/apps/niles-chores-web/public/config.template.json \
   app/client/apps/niles-chores-web/public/config.json

# Defaults in the templates are correct for local dev — edit only if ports differ.

# 3. Install frontend deps
cd app && pnpm install
```

### Starting from Visual Studio

Open `Bureau.slnx`. In the launch profile dropdown select one of:

| Profile | Starts |
|---------|--------|
| `Bureau[Dev]` | Niles.Chores.Api + Watson.Nodes.Api + frontend |
| `Niles.Chores[Dev]` | Niles.Chores.Api + frontend |
| `Watson.Nodes[Dev]` | Watson.Nodes.Api + frontend |

Press **F5**. Visual Studio starts the selected APIs and launches `Bureau.Frontend.DevRunner` in its own console window. The DevRunner runs `pnpm dev` from `app/`, which starts Vite for bureau-web (and any other workspace packages that have a `dev` script) in parallel.

Navigate to `http://localhost:5000`.

### Starting from the terminal

```bash
# APIs (each in its own terminal):
dotnet run --project server/src/Niles.Chores.Api    # :5300
dotnet run --project server/src/Watson.Nodes.Api    # :5200

# Frontend:
cd app && pnpm dev  # starts bureau-web at :5000
```

---

## 3. Port assignments

| Port | Service | Domain |
|------|---------|--------|
| 5000 | bureau-web (Vite dev) — admin shell | Frontend |
| 5001 | niles-chores-web (Vite dev) — daily chores | Frontend |
| 5100 | Sven (auth, TBD) | Auth |
| 5200 | Watson.Nodes.Api | Watson |
| 5210 | Watson.Items.Ingest.Api | Watson |
| 5300 | Niles.Chores.Api | Niles |

**Rule for new APIs:** find the domain block (`52XX` for Watson, `53XX` for Niles), take the
next unused `X0` slot (5200 → 5210 → 5220). Update this table and `launchSettings.json`.

---

## 4. Docker build (bureau-web)

Build context must be the **repo root** (required to COPY `app/` and `server/deploy/`).

```bash
# From repo root:
docker build \
  -f app/client/apps/bureau-web/Dockerfile \
  -t bureau-web:latest \
  .
```

Run with environment variables:
```bash
docker run -p 80:80 \
  -e BUREAU_ENVIRONMENT=test \
  -e BUREAU_AUTH_MODE=dev \
  -e BUREAU_API_CHORES=http://niles-chores-api \
  -e BUREAU_API_NODES=http://watson-nodes-api \
  -e BUREAU_API_ITEMS_INGEST=http://watson-items-ingest-api \
  bureau-web:latest
```

The Nginx entrypoint runs `envsubst` to generate `config.json` from `config.template.json`
before Nginx starts. Nginx serves `config.json` with `Cache-Control: no-store`.

### Required environment variables

| Variable | Description | Example |
|----------|-------------|---------|
| `BUREAU_ENVIRONMENT` | Environment name | `test`, `production` |
| `BUREAU_AUTH_MODE` | `dev` or `oidc` | `dev` |
| `BUREAU_API_CHORES` | Chores API base URL | `http://niles-chores-api` |
| `BUREAU_API_NODES` | Nodes API base URL | `http://watson-nodes-api` |
| `BUREAU_API_ITEMS_INGEST` | Items ingest API base URL | `http://watson-items-ingest-api` |
| `BUREAU_OIDC_AUTHORITY` | Sven authority URL (oidc mode only) | `https://auth.bureau.home` |
| `BUREAU_OIDC_CLIENT_ID` | OIDC client ID (oidc mode only) | `bureau-web` |
| `BUREAU_OIDC_REDIRECT_URI` | OIDC redirect URI (oidc mode only) | `https://bureau.home/callback` |

---

## 5. Test environment (docker compose)

```bash
cd server/deploy
cp .env.template .env   # create if not exists
docker compose up -d
```

Services in `docker-compose.yml`:
- `niles-chores-api` — Chores REST API
- `bureau-web` — Nginx SPA (bureau-web)

All services connect via `nuc-network` (external Docker network).

---

## 6. Prod environment (Proxmox)

Target layout on Proxmox:

```
proxmox LXC/VM containers:
├── sqlserver          (SQL Server 2022)
├── sven               (OIDC auth server — feature/sven, port 5100)
├── niles-chores-api   (Chores API, port 5300)
├── watson-nodes-api   (Nodes API, port 5200)
└── bureau-web         (Nginx SPA, port 80)

Nginx reverse proxy (host):
  bureau.home      → bureau-web:80
  auth.bureau.home → sven:5100
  api.chores.home  → niles-chores-api:5300
  api.nodes.home   → watson-nodes-api:5200
```

Set `BUREAU_AUTH_MODE=oidc` and configure the OIDC variables once Sven is merged.

---

## 7. CORS

In dev, CORS is set to `*` (existing behaviour in API `appsettings.Development.json`).

In prod/test, set `Cors__AllowedOrigins__0` to the bureau-web origin
(e.g. `https://bureau.home`) in each API's environment configuration.
