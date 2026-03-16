---
phase: 04-end-session-security
plan: 04
subsystem: auth
tags: [oidc, end-session, rp-initiated-logout, jwt-validation, security]

# Dependency graph
requires:
  - phase: 04-03
    provides: PostLogoutRedirectUris wired through registration pipeline to stored Client
provides:
  - EndSessionController JWT signature + issuer validation before session clear
  - EndSessionController registration-based redirect URI check (no open-redirect)
  - EndSessionEndpoint property on Sven.Models.DiscoveryDocument
  - EndSessionEndpoint populated in DiscoveryService constructor
  - Endpoints.Connect.EndSession constant added to Sven/Configurations/Endpoints.cs
  - Four passing PROT-04 integration tests
affects:
  - DiscoveryDocument (well-known response now includes end_session_endpoint)
  - EndSessionController (PROT-04 security fix complete)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "JWT hint validation with ValidateLifetime=false — expired id_token_hint still accepted per OIDC Session §5"
    - "Session cleared ONLY after both JWT signature/issuer and client lookup pass — prevents session-clear forgery"
    - "Redirect URI accepted only when present in client.PostLogoutRedirectUris (exact Contains match)"
    - "No-hint path clears session unconditionally and returns 200"

key-files:
  created: []
  modified:
    - server/src/Sven.Web/Controllers/Connect/EndSessionController.cs
    - server/src/Sven/Models/DiscoveryDocument.cs
    - server/src/Sven/Services/DiscoveryService.cs
    - server/src/Sven/Configurations/Endpoints.cs
    - server/tests/Sven.Tests/EndSession/RpInitiatedLogoutTests.cs

key-decisions:
  - "Endpoints.Connect.EndSession added to Sven/Configurations/Endpoints.cs (mirrors Sven.Contracts/Endpoints.cs) — DiscoveryService is in Sven project, cannot reference Sven.Contracts"
  - "ValidateLifetime = false in TokenValidationParameters — OIDC Session §5 requires servers to accept expired id_token_hint values"
  - "IClientService injected normally via constructor (public interface) — no RequestServices workaround needed unlike IClientAuthService"

patterns-established:
  - "JWT hint validation reuses TokenValidationParameters pattern from SvenTokenProvider.IntrospectAsync with ValidateLifetime=false variant"

requirements-completed: [PROT-04]

# Metrics
duration: 15min
completed: 2026-03-16
---

# Phase 4 Plan 4: PROT-04 End-Session Security Fix Summary

**PROT-04 complete: EndSessionController now validates JWT signature + issuer before clearing session, rejects unregistered redirect URIs, and advertises end_session_endpoint in discovery**

## Performance

- **Duration:** ~15 min
- **Completed:** 2026-03-16
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- **Hardened EndSessionController** (`Sven.Web`): replaced unconditional `SignOutAsync` with a gate that validates JWT signature and issuer before clearing any session state; invalid hints return 400 without touching the session
- **Open-redirect closed**: removed `IsValidUri` syntactic check; `post_logout_redirect_uri` is now accepted only when present in `client.PostLogoutRedirectUris` (exact `Contains` match)
- **No-hint path preserved**: request with no `id_token_hint` still clears session and returns 200 (anonymous logout)
- **Discovery document updated**: `EndSessionEndpoint` property added to `Sven.Models.DiscoveryDocument`; `DiscoveryService` populates it with `{issuer}/connect/endsession`
- **`Endpoints.Connect.EndSession` added** to `Sven/Configurations/Endpoints.cs` (mirrors `Sven.Contracts/Endpoints.cs`; needed by `DiscoveryService`)
- **All four PROT-04 integration tests pass**: SC1 (invalid hint → 400), SC2 (unregistered URI → 200), SC3 (registered URI → 302), SC4 (no hint → 200)

## Task Commits

1. **Task 1 — `36a2205`**: `feat(04-04): harden EndSessionController + add EndSessionEndpoint to discovery`
2. **Task 2 — `4067129`**: `feat(04-04): implement RpInitiatedLogoutTests — all four PROT-04 scenarios green`

## Files Created/Modified

- `server/src/Sven.Web/Controllers/Connect/EndSessionController.cs` — complete replacement: JWT validation gate, `IClientService` + `IOptions<JwtOptions>` + `RsaSecurityKey` injections, registration-based redirect check, `IsValidUri` removed
- `server/src/Sven/Models/DiscoveryDocument.cs` — added `EndSessionEndpoint` auto-property with `[JsonPropertyName("end_session_endpoint")]`
- `server/src/Sven/Services/DiscoveryService.cs` — added `EndSessionEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.EndSession}"` to object initializer
- `server/src/Sven/Configurations/Endpoints.cs` — added `EndSessionPath = "endsession"` and `EndSession = "/connect/endsession"` constants
- `server/tests/Sven.Tests/EndSession/RpInitiatedLogoutTests.cs` — all four test methods implemented; helpers `SeedClientWithLogoutUriAsync`, `BuildValidIdTokenHintAsync`, `BuildWrongKeyIdTokenHint` added

## Decisions Made

- `Endpoints.Connect.EndSession` constant was already present in `Sven.Contracts/Endpoints.cs` (added in plan 04-01) but not in `Sven/Configurations/Endpoints.cs`. Since `DiscoveryService` is in the `Sven` project, the constant must exist there too. Added it to mirror the existing definition.
- `ValidateLifetime = false` is intentional per OIDC Session Management §5 — expired `id_token_hint` values must be accepted to allow logout after token expiry.
- `IClientService` injected via constructor (not `HttpContext.RequestServices`) because it is a public interface; no `CS0051` accessibility constraint applies.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Missing Endpoints.Connect.EndSession in Sven/Configurations/Endpoints.cs**
- **Found during:** Task 1
- **Issue:** `DiscoveryService.cs` references `Endpoints.Connect.EndSession` (from `Sven` project's `Endpoints.cs`), but that constant was only defined in `Sven.Contracts/Endpoints.cs`. Build would fail if not added.
- **Fix:** Added `EndSessionPath = "endsession"` and `EndSession = $"/{Base}/{EndSessionPath}"` to `Sven/Configurations/Endpoints.cs` matching the existing `Sven.Contracts` definition.
- **Files modified:** `server/src/Sven/Configurations/Endpoints.cs`
- **Commit:** `36a2205`

## Test Results

- EndSession category: **4 passed, 0 failed**
- Full suite: 29 failures (pre-existing — same failures existed before these changes; 4 EndSession stubs converted from failing to passing)

## Self-Check: PASSED

- `server/src/Sven.Web/Controllers/Connect/EndSessionController.cs` — FOUND
- `server/src/Sven/Models/DiscoveryDocument.cs` — FOUND, contains `EndSessionEndpoint`
- `server/src/Sven/Services/DiscoveryService.cs` — FOUND, contains `EndSessionEndpoint =`
- `server/src/Sven/Configurations/Endpoints.cs` — FOUND, contains `EndSession`
- `server/tests/Sven.Tests/EndSession/RpInitiatedLogoutTests.cs` — FOUND, contains `RegisteredRedirectUri_Returns302`
- Commit `36a2205` — FOUND
- Commit `4067129` — FOUND

---
*Phase: 04-end-session-security*
*Completed: 2026-03-16*
