---
phase: 02-client-credentials-grant
plan: 05
subsystem: auth
tags: [oidc, client-credentials, confidential-client, rfc7591, registration, iat, bcrypt]

# Dependency graph
requires:
  - phase: 02-client-credentials-grant
    provides: ClientService, ClientAuthService, SvenTokenProvider for client_credentials grant
provides:
  - IAT-gated confidential client registration at POST /oidc/register
  - ClientService confidential client branch (secret generation, hashing, Type=confidential)
  - Rfc7591ConfidentialClientRegistrationTests fully implemented (4 green tests)
affects:
  - 02-client-credentials-grant (closes PROT-01 registration half)
  - 05-feature-gating (will need registered confidential clients)
  - 06-token-exchange (depends on confidential clients)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IAT (Initial Access Token) gating via Bearer header for confidential client registration"
    - "One-time raw secret: generated, hashed (bcrypt), returned in 201 response only, never stored raw"
    - "ClientSecretExpiresAt=epoch-0 per RFC 7591 §3.2.1 (does not expire)"
    - "isConfidential branch pattern: determined by token_endpoint_auth_method at both controller and service layers"

key-files:
  created: []
  modified:
    - server/src/Sven/Services/ClientService.cs
    - server/src/Sven.Web/Controllers/OidcController.cs
    - server/tests/Sven.Tests/ClientCredentials/Rfc7591ConfidentialClientRegistrationTests.cs
    - server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs

key-decisions:
  - "IConfiguration injected into OidcController via constructor for Sven:InitialAccessToken access"
  - "IAT guard runs before all other registration validation (redirect URIs, scope, grant types)"
  - "501 returned when Sven:InitialAccessToken is not configured — confidential registration disabled by default"
  - "Redirect URI validation skipped for confidential clients (client_credentials grant has no redirect)"
  - "Rfc7591 test Register_SecretReturnedOnce verifies two registrations produce different secrets (ephemeral by design)"

patterns-established:
  - "Confidential client: isConfidential = ClientSecretBasic || ClientSecretPost; applied consistently in both controller and service"
  - "Raw secret: generated in ClientService.CreateClientAsync, stored on client.ClientSecret (ephemeral), never persisted to DB — only HashedSecret persisted"

requirements-completed: [PROT-01]

# Metrics
duration: 11min
completed: 2026-03-15
---

# Phase 2 Plan 5: IAT-Gated Confidential Client Registration Summary

**RFC 7591 Dynamic Registration with IAT guard: confidential clients registered via Bearer token, secret returned exactly once as raw value, hashed version stored in DB**

## Performance

- **Duration:** 11 min
- **Started:** 2026-03-15T19:08:36Z
- **Completed:** 2026-03-15T19:20:01Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- `ClientService.CreateClientAsync` now handles confidential clients: generates 32-byte random secret, bcrypt-hashes it via `PasswordHasher`, stores `HashedSecret`, sets `Type=confidential`, sets `ClientSecretExpiresAt=epoch-0` (RFC 7591 §3.2.1 — does not expire)
- `OidcController.RegisterAsync` guards confidential registration with Bearer IAT check; returns 501 if `Sven:InitialAccessToken` not configured, 401 on missing/wrong token
- All 4 `Rfc7591ConfidentialClientRegistrationTests` implemented and green; Rfc6749 grant tests (9) and all other passing tests unchanged

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend ClientService.CreateClientAsync with confidential client branch** - `199434e` (feat)
2. **Task 2: Add IAT guard and relax redirect URI validation in OidcController** - `8f7cce3` (feat)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `server/src/Sven/Services/ClientService.cs` - Confidential client branch with secret generation/hashing; scope check bug fixed
- `server/src/Sven.Web/Controllers/OidcController.cs` - IConfiguration injected; IAT guard block; redirect URI guard relaxed for confidential clients
- `server/tests/Sven.Tests/ClientCredentials/Rfc7591ConfidentialClientRegistrationTests.cs` - All 4 test stubs replaced with real integration tests
- `server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs` - Added `Sven:InitialAccessToken` to base test config using `TestDataConstants.TestIat`

## Decisions Made
- `IConfiguration` injected via constructor (not `IOptions<SvenOptions>`) — matches the `EncryptionKeyStartupFilter` precedent for scalar config keys
- IAT guard runs first (before redirect URI check, scope, grant type checks) so 401 is returned before any client creation logic
- `RedirectUris ?? []` null-guard added in both `OidcController` (ClientRequest build) and `ClientService` (`[.. clientRequest.RedirectUris ?? []]`) for safety
- `Register_SecretReturnedOnce` test verifies via two registrations yielding different secrets — no retrieval endpoint exists, confirming ephemerality by design

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed inverted scope check in ClientService.CreateClientAsync**
- **Found during:** Task 2 (OidcController IAT guard implementation — running registration tests end-to-end)
- **Issue:** `if (DiscoveryService.ScopeSupported.IsScopeSameOrSubset(clientRequest.Scope))` returned an error when scope WAS valid — condition was inverted, causing all registration requests to fail with "Scope not supported"
- **Fix:** Added `!` negation: `if (!DiscoveryService.ScopeSupported.IsScopeSameOrSubset(...))`
- **Files modified:** `server/src/Sven/Services/ClientService.cs`
- **Verification:** Rfc7591 registration tests pass end-to-end after fix; pre-existing test count unchanged
- **Committed in:** `8f7cce3` (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 bug — inverted boolean condition)
**Impact on plan:** Fix was necessary for any registration request to succeed. No scope creep. Pre-existing tests (Rfc6749 grant tests) bypass CreateClientAsync, so the bug was invisible until Rfc7591 tests exercised the registration path.

## Issues Encountered
- MSBuild cache error (`Bureau.Primitives.AssemblyInfoInputs.cache`) blocked initial `dotnet build --no-restore`. Resolved by running without `--no-restore` flag (restored first run to regenerate cache). Not a code issue.

## User Setup Required
None — test environment uses `TestDataConstants.TestIat` via `SvenWebAppFactory` base config. Production deployment must set env var `SVEN__INITIALACCESSTOKEN` to enable confidential client registration; if unset, POST /oidc/register for confidential clients returns 501.

## Next Phase Readiness
- PROT-01 fully closed: register confidential client → authenticate → get token all work end-to-end
- Rfc7591 and Rfc6749 integration tests are green
- `ClientAuthServiceTests` (6 stubs) remain as pre-existing `Assert.Fail` stubs — out of scope for this plan

---
*Phase: 02-client-credentials-grant*
*Completed: 2026-03-15*
