# Sven

## What This Is

Sven is Bureau's self-hosted OAuth 2.0 / OpenID Connect identity provider and token broker. It
authenticates household users (via local credentials or linked external providers), issues signed
JWTs for all Bureau apps, and acts as a token vault — storing, refreshing, and brokering external
provider tokens (Google, Microsoft, etc.) so Bureau apps can access cloud APIs on behalf of users
without ever handling external refresh tokens themselves.

## Core Value

Bureau apps authenticate against one system (Sven) and request external tokens from one system
(Sven) — no Bureau app implements provider-specific OAuth, and no external secret leaves Sven.

## Requirements

### Validated

- ✓ Authorization Code + PKCE (RFC 7636, S256 required) — existing
- ✓ Token revocation (RFC 7009) — existing
- ✓ JWKS endpoint (RFC 7517) — existing
- ✓ Dynamic client registration (RFC 7591) — existing
- ✓ OIDC Discovery document — existing
- ✓ Google external login — existing
- ✓ Multiple external provider accounts per user (same provider, different emails) — existing
- ✓ AES-256-GCM encrypted token storage — existing
- ✓ Background token refresh service — existing
- ✓ Household identity claim in JWT — existing (data model only)

### Active

- [ ] Clean project layering: Sven.Web / Sven.Abstractions / Sven / Sven.Data.Postgres
- [ ] Sven.Abstractions contains only interfaces Sven.Web depends on
- [ ] Sven.Contracts contains HTTP boundary types (routes, req/res DTOs) visible to external clients
- [ ] All services named `*Service` / `I*Service`; repositories named `*Repository` / `I*Repository`; internal where appropriate
- [ ] Client Credentials grant (RFC 6749 §4.4) for service-to-service and background daemons
- [ ] RFC 8693 Token Exchange endpoint — token broker for external provider tokens
- [ ] OIDC UserInfo endpoint (OIDC Core §5.3)
- [ ] RP-initiated logout / end_session (OIDC Session §5)
- [ ] Token introspection (RFC 7662)
- [ ] Microsoft external login (fully tested)
- [ ] Modular external provider system — adding a new provider requires only a new provider module, no core changes
- [ ] RFC-traceable code — code structure maps clearly to RFC flows so a reader can follow auth code flow, token exchange, etc. in code
- [ ] RFC compliance test suite — tests named and structured to spec sections; failing test = spec violation
- [ ] Tests for token vault: storage, background refresh, on-demand refresh, revocation
- [ ] Tests for multi-provider and multi-account-per-provider scenarios
- [ ] Useful documentation covering both RFC standards and Sven's specific implementation decisions
- [ ] Security gaps resolved: cryptographic verification codes, rate limiting, audit log

### Out of Scope

- Implicit flow — intentionally excluded (security)
- Resource Owner Password Credentials — intentionally excluded (security)
- Device Code flow — not needed for Bureau's use cases
- Multi-tenancy — Bureau is single-household
- Cloud hosting / SaaS — self-hosted only

## Context

Sven is part of the Bureau home-server umbrella. Bureau runs on Proxmox and also integrates with
self-hosted services (paperless-ngx, Immich) and cloud APIs (Google Drive/Calendar/Mail,
Microsoft OneDrive/Calendar/Mail). Bureau apps are: bureau-web (SPA, admin + household features),
mobile app, and service APIs (Watson.Nodes, Watson.Items, Niles.Chores). Apps communicate with
each other and with Sven.

Sven is already partially implemented on the `feature/sven` branch. A refactoring is in progress:
old patterns (Providers, Stores, file-scoped namespaces) are being replaced with the target
patterns (Services, Repositories, block-scoped namespaces). Several planned RFCs (token exchange,
introspection, end_session) are not yet implemented.

Sign-in and account management UI are Razor Pages served by Sven itself — required so that
ASP.NET Cookie Authentication sessions work on the same domain as OIDC endpoints.

## Constraints

- **Tech stack**: ASP.NET Core, Entity Framework Core, PostgreSQL (primary), SQL Server (supported)
- **RFC-first**: every protocol behaviour must conform to the referenced spec; most secure option chosen where specs allow choice
- **Deployment**: internet-exposed at `auth.bureau.home`; HTTPS enforced in production
- **Security**: PKCE S256 required for all clients; no implicit flow; exact redirect URI match; external refresh tokens never returned to clients
- **Coding conventions**: block-scoped namespaces, explicit types (no `var`), `Async` suffix + `CancellationToken`, explicit `get`/`set` property bodies

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Sven.Abstractions = Sven.Web's contract only | Prevents Sven.Web from depending on internal Sven business logic; keeps the HTTP layer thin | — Pending |
| Sven.Contracts = external client-visible types | Same pattern as Watson/Niles Contracts; Bureau apps reference this to call Sven's API | — Pending |
| Modular external providers | New provider = new module; no core changes required | — Pending |
| Client Credentials for service-to-service | Bureau apps and daemons need machine tokens without user context | — Pending |
| RFC-traceable code structure | Code should be readable alongside the RFCs; method/class names reference spec terminology | — Pending |
| Tests as RFC compliance record | Each RFC flow has a named test suite; a failing test = a spec violation, not just a bug | — Pending |

---
*Last updated: 2026-03-15 after initialization*
