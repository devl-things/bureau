# External Integrations

**Analysis Date:** 2026-03-15

## APIs & External Services

**Authentication & Identity:**
- Sven (self-hosted OIDC)
  - Purpose: Central auth server for household users, feature scope distribution
  - Implementation: OpenId Connect with JWT tokens
  - Config env vars: `BUREAU_OIDC_AUTHORITY`, `BUREAU_OIDC_CLIENT_ID`, `BUREAU_OIDC_REDIRECT_URI`

**External OAuth Providers (Sven.Web):**
- Google
  - Package: `Microsoft.AspNetCore.Authentication.Google`
  - Purpose: Sign-in alternative (deprecated after web restructuring)

- Microsoft Account
  - Package: `Microsoft.AspNetCore.Authentication.MicrosoftAccount`
  - Purpose: Sign-in alternative (deprecated after web restructuring)

**Backend APIs (Internal):**
- Niles.Chores.Api (Watson.Nodes.Api will have similar pattern)
  - Port: 5300 (local dev)
  - Endpoint configured via env var: `BUREAU_API_CHORES`
  - Auth: JWT Bearer (feature scopes: `niles.chores`)

- Watson.Nodes.Api
  - Port: 5200 (local dev)
  - Endpoint configured via env var: `BUREAU_API_NODES`
  - Auth: JWT Bearer (feature scopes: `watson.nodes`, `watson.nodes.crud`)

- Watson.Items.Ingest.Api
  - Port: 5210 (local dev)
  - Endpoint configured via env var: `BUREAU_API_ITEMS_INGEST`
  - Auth: JWT Bearer

## Data Storage

**Databases:**
- SQL Server (default)
  - Connection: `ConnectionStrings__NilesDb` (Chores)
  - Connection: `ConnectionStrings__WatsonNodesDb` (Nodes)
  - Client: Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL
  - Support: Development via Docker, production on Proxmox

- PostgreSQL (alternative provider available)
  - Client: Entity Framework Core 9.0 + Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
  - Location: `server/src/Sven.Data.Postgres/`

- In-Memory Database (testing)
  - Framework: Microsoft.EntityFrameworkCore.InMemory
  - Usage: Test fixtures via `SvenWebAppFactory<T>`

**File Storage:**
- Local filesystem only — no S3 or cloud storage configured

**Caching:**
- In-memory stores for authentication codes and requests
  - `InMemoryStore<string, AuthCode>` — Auth code cache (Sven)
  - `InMemoryStore<string, OAuthRequest>` — OAuth request state (Sven)
  - `InMemoryStore<string, UserVerificationCode>` — Verification codes (Sven)

**Local SQLite (Mobile):**
- expo-sqlite — Local data persistence on mobile device

## Authentication & Identity

**Auth Provider:**
- Sven (custom self-hosted OIDC)
  - Implementation: `Sven.Web` project (Razor Pages hosting auth flows)
  - Repository: Sven.Data.SqlServer and Sven.Data.Postgres (database abstraction)

**Token Handling:**
- JWT tokens with RS256 signing
  - RSA 2048-bit key generated per instance
  - Token types: `IdToken`, `AccessToken`, `RefreshToken`
  - Lifetime config: `Jwt__IdTokenLifetime`, `Jwt__AccessTokenLifetime`, `Jwt__RefreshTokenLifetime`
  - Location: `server/src/Sven/Services/SvenTokenProvider.cs`

**Feature-Based Access Control:**
- OAuth 2.0 scopes encoded in JWT
  - Defined in: `server/src/Sven.Abstractions/Models/ExternalScope.cs`
  - Scopes propagated to dependent services via claims
  - Example scopes: `watson.nodes`, `watson.nodes.crud`, `niles.chores`

**External Token Management (For External Providers):**
- `IExternalTokenService` — Stores user's external provider tokens
  - Location: `server/src/Sven.Abstractions/Data/IExternalTokenService.cs`
  - Purpose: Token exchange and refresh for linked identities

- `ExternalTokenRefresher` — Background service for token refresh
  - Location: `server/src/Sven/Services/ExternalTokenRefresher.cs`
  - Pattern: HttpClient-based refresh before token expiry

## Monitoring & Observability

**Error Tracking:**
- None configured (not detected)

**Logs:**
- Console/file via Microsoft.Extensions.Logging
- Activity tracking middleware: `Bureau.AspNetCore.Logging.AddBureauActivityTracking()`
- Location: `server/src/Bureau.AspNetCore/`
- Format: Structured, respects `Logging__LogLevel__*` config

**Health Checks:**
- Endpoint: `GET /health` (ApiRoutes.Health.Root)
- Checks include: Database connectivity, service readiness
- Location: `Niles.Chores.Api` example at `server/src/Niles.Chores.Api/Program.cs:39-40`

## CI/CD & Deployment

**Hosting:**
- Docker containers on Proxmox home server
- Nginx reverse proxy for bureau-web SPA
- External network: `nuc-network` (defined in docker-compose.yml)

**CI Pipeline:**
- Not detected (no GitHub Actions or CI config in repo)

**Containerization:**
- `Dockerfile` locations:
  - `app/client/apps/bureau-web/Dockerfile` — Multi-stage Node/Nginx build
  - `server/src/Sven/Dockerfile` — Standard .NET 8 build/runtime
  - `server/deploy/Dockerfile.chores.api` — Niles.Chores.Api
  - `server/deploy/Dockerfile.chores.web` — (deprecated, removed in restructuring)

**Docker Compose:**
- Dev: `server/deploy/docker-compose.dev.yml` — Local development (SQL Server)
- Prod: `server/deploy/docker-compose.yml` — Production deployment

## Environment Configuration

**Required env vars (backend):**
- `ASPNETCORE_ENVIRONMENT` — `Development|Test|Production`
- `Auth__Mode` — `None|Dev|Oidc` (for API auth middleware)
- `ConnectionStrings__*` — Database connection strings
- `Cors__AllowedOrigins__*`, `Cors__AllowedHeaders__*`, `Cors__AllowedMethods__*` — CORS policy
- `Logging__LogLevel__*` — Log level configuration

**Required env vars (frontend):**
- `BUREAU_ENVIRONMENT` — `test|production`
- `BUREAU_AUTH_MODE` — `dev|oidc`
- `BUREAU_OIDC_AUTHORITY` — Sven authority URL
- `BUREAU_OIDC_CLIENT_ID` — Registered client ID in Sven
- `BUREAU_OIDC_REDIRECT_URI` — Post-login redirect (e.g., `http://localhost:5000/callback`)
- `BUREAU_API_CHORES` — Niles.Chores.Api base URL
- `BUREAU_API_NODES` — Watson.Nodes.Api base URL
- `BUREAU_API_ITEMS_INGEST` — Watson.Items.Ingest.Api base URL

**Secrets location:**
- Dev: User secrets via `dotnet user-secrets` (Sven.Web has ID `84fc99e2-8226-40a7-b3ae-cbe5ad97935b`)
- Prod: Docker secrets or environment variables passed at container runtime
- Not committed: `.env` files, `appsettings.*.local.json`

## Webhooks & Callbacks

**Incoming:**
- OAuth authorize callback: `/connect/callback` (Sven)
- Token exchange: `/connect/token` (Sven)
- User verification: `/verification` endpoints (Sven)

**Outgoing:**
- None detected (no webhook dispatch to external services)

**Internal Callbacks:**
- Frontend redirect after auth: `BUREAU_OIDC_REDIRECT_URI`
- API CORS pre-flight: Handles via middleware in `Bureau.AspNetCore`

## Planned Integrations (Per VISION.md)

**Future external APIs (not yet implemented):**
- Immich — Photo/video library (feature scope: `immich.*`)
- paperless-ngx — Document management (feature scope: `paperless.*`)
- Home Assistant — Automation (feature scope: `homeassistant.*`)
- Jellyfin — Media (feature scope: `jellyfin.*`)

Integration pattern: Each appears as a feature module in bureau-web behind its own scope; users without scope don't see it.

---

*Integration audit: 2026-03-15*
