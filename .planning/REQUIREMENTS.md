# Requirements: Sven

**Defined:** 2026-03-15
**Core Value:** Bureau apps authenticate against one system (Sven) and request external tokens from one system (Sven) — no Bureau app implements provider-specific OAuth, and no external secret leaves Sven.

## v1 Requirements

### Security Hardening

- [x] **SEC-01**: Verification codes are generated with `RandomNumberGenerator` (not `System.Random`) and expire after 5 minutes
- [x] **SEC-02**: Rate limiting is applied to `/connect/token`, `/connect/authorize`, and sign-in endpoints using ASP.NET Core built-in middleware
- [x] **SEC-03**: Authorization code invalidation is atomic — a single database DELETE with row-count check; any reuse attempt is logged as a security event
- [x] **SEC-04**: AES-256 encryption key is loaded from environment variable or secrets manager, not from `appsettings.json`

### Protocol Completeness

- [x] **PROT-01**: Client application can obtain an access token via the Client Credentials grant (`grant_type=client_credentials`) using `client_secret_basic` or `client_secret_post` authentication
- [x] **PROT-02**: Resource server can validate any Sven-issued token via `POST /oidc/introspect`; the `introspection_endpoint` is advertised in the discovery document
- [ ] **PROT-03**: Bureau app can exchange a Sven access token for an external provider access token via `POST /connect/token` with `grant_type=urn:ietf:params:oauth:grant-type:token-exchange`; all six RFC 8693 subject token validation steps are enforced
- [x] **PROT-04**: RP-initiated logout validates `id_token_hint` against the issuer and validates `post_logout_redirect_uri` by exact string match against registered client URIs; logout completes even if the redirect URI is absent or invalid

### Token Vault

- [ ] **VAULT-01**: Client application can include a `bureau_features` field in dynamic registration (`POST /oidc/register`) declaring which Bureau feature scopes it exposes and which external provider scopes each feature requires; Sven persists and validates this data
- [ ] **VAULT-02**: Each external provider (Google, Microsoft, future) is implemented as a self-contained module (`IExternalProviderModule`); adding a new provider requires only a new module with no changes to core token refresh or token exchange code
- [ ] **VAULT-03**: Sven access tokens for household members include `household_id` and `household_role` JWT claims sourced from the live database
- [ ] **VAULT-04**: Token exchange falls back to a household member's shared external token when the requesting user has no token for the requested feature; `IHouseholdService` is fully implemented

### Code Structure

- [x] **CODE-01**: `Sven.Abstractions` contains only interfaces and models that `Sven.Web` depends on; `Sven.Contracts` contains HTTP boundary types (routes, request/response DTOs) for external callers; `Sven` implements `Sven.Abstractions`; no cross-layer reference violations
- [x] **CODE-02**: All service classes are named `*Service` implementing `I*Service`; all repository classes are named `*Repository` implementing `I*Repository`; classes used only within `Sven` are `internal`
- [ ] **CODE-03**: Service and controller classes carry XML doc comments referencing the relevant RFC section for each operation (e.g., `/// RFC 6749 §4.1 — Authorization Code Grant`); method names are clear and domain-readable
- [ ] **CODE-04**: Token issuance, exchange, and error events are logged via `ILogger` with structured message templates compatible with future Serilog adoption; no external logging library dependency required

### Tests

- [ ] **TEST-01**: Each implemented RFC has a corresponding test class named after its spec section (e.g., `Rfc6749AuthorizationCodeGrantTests`); each `MUST`/`SHALL` requirement in the spec has at least one test; a failing test means a spec violation
- [ ] **TEST-02**: Test suite covers linking multiple accounts from the same external provider to one Sven user, and sign-in via each linked account
- [ ] **TEST-03**: Test suite covers external token storage, background refresh, on-demand fallback refresh, and token revocation paths
- [ ] **TEST-04**: Every supported grant type has an end-to-end integration test using `WebApplicationFactory`; tests pass on both Postgres and SQL Server data providers

### Documentation

- [ ] **DOCS-01**: Implementation guide documents how each RFC flow is implemented in Sven, with code pointers to the relevant service and controller methods
- [ ] **DOCS-02**: `docs/SVEN.md` is updated to reflect all completed RFC statuses, all active endpoints, all design decisions made during this work
- [ ] **DOCS-03**: External provider guide documents step-by-step how to add a new `IExternalProviderModule` to Sven
- [ ] **DOCS-04**: Security model document covers all security decisions, known trade-offs, resolved gaps from `SVEN.md §9.3`, and operational security notes (key management, rate limits, audit logging)

---

## v2 Requirements

### Infrastructure

- **INFRA-01**: JWT signing key is persisted across restarts (ASP.NET Core Data Protection or environment-injected) so Sven restarts do not invalidate all active sessions
- **INFRA-02**: Refresh token rotation grace period is configurable to tolerate network hiccups without silent session loss

### External Providers

- **PROV-01**: Facebook provider module implemented as `IExternalProviderModule`
- **PROV-02**: AES-256 encryption key rotation with overlap window so existing tokens remain decryptable

### Household

- **HOUSE-01**: Household creation, invite, and accept flow implemented end-to-end
- **HOUSE-02**: Sharing consent UI — account management page for per-feature external token sharing with household members
- **HOUSE-03**: Per-feature sharing granularity in the sharing consent UI

---

## Out of Scope

| Feature | Reason |
|---------|--------|
| Implicit flow | Intentionally excluded — insecure, not RFC 8252 compliant |
| Resource Owner Password Credentials | Intentionally excluded — phishing risk, not RFC 9700 compliant |
| Device Code flow | No Bureau use case requires it |
| Multi-tenancy | Bureau is single-household |
| Cloud hosting / SaaS | Self-hosted only |

---

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| SEC-01 | Phase 1 | Complete |
| SEC-02 | Phase 1 | Complete |
| SEC-03 | Phase 1 | Complete |
| SEC-04 | Phase 1 | Complete |
| CODE-01 | Phase 1 | Complete |
| CODE-02 | Phase 1 | Complete |
| PROT-01 | Phase 2 | Complete |
| PROT-02 | Phase 3 | Complete |
| PROT-04 | Phase 4 | Complete |
| VAULT-01 | Phase 5 | Pending |
| PROT-03 | Phase 6 | Pending |
| VAULT-03 | Phase 6 | Pending |
| VAULT-04 | Phase 6 | Pending |
| VAULT-02 | Phase 7 | Pending |
| TEST-01 | Phase 8 | Pending |
| TEST-02 | Phase 8 | Pending |
| TEST-03 | Phase 8 | Pending |
| TEST-04 | Phase 8 | Pending |
| DOCS-01 | Phase 8 | Pending |
| DOCS-02 | Phase 8 | Pending |
| DOCS-03 | Phase 8 | Pending |
| DOCS-04 | Phase 8 | Pending |
| CODE-03 | Phase 8 | Pending |
| CODE-04 | Phase 8 | Pending |

**Coverage:**
- v1 requirements: 24 total
- Mapped to phases: 24
- Unmapped: 0

---
*Requirements defined: 2026-03-15*
*Last updated: 2026-03-15 after 01-02 plan completion (CODE-01, CODE-02 marked complete)*
