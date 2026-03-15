---
phase: 02-client-credentials-grant
plan: 03
subsystem: auth
tags: [oauth2, client-credentials, client-auth, discovery, asp-net-core]

# Dependency graph
requires:
  - phase: 02-client-credentials-grant
    provides: HashedSecret on Client model and ClientDb, repository round-trip (plan 02-02)
  - phase: 01-security-hardening
    provides: PasswordHasher, ResultError/Result<T> railway pattern, ILogger structured logging
provides:
  - IClientAuthService interface (internal, HttpRequest -> Result<Client>)
  - ClientAuthService implementation with Basic + Post extraction and all error paths
  - AuthConstants.GrantTypes.ClientCredentials constant
  - DiscoveryService accepts client_secret_basic, client_secret_post, client_credentials
  - TokenRequest.RedirectUri [Required] removed; TokenRequest.ClientSecret field added
affects:
  - 02-04 (HandleClientCredentialsFlow needs IClientAuthService ready for DI injection)
  - 02-05 (OidcController confidential client registration uses DiscoveryService.TryGetSupportedAuthMethod)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IClientAuthService: narrow service — HttpRequest in, Result<Client> out; token controller stays thin"
    - "Dual-method check before credential extraction per RFC 6749 §2.3"
    - "TryExtractBasicCredentials splits on first colon (IndexOf) per RFC 7617 §2"
    - "PasswordHasher.VerifyPassword for timing-safe secret comparison"

key-files:
  created:
    - server/src/Sven/Services/IClientAuthService.cs
    - server/src/Sven/Services/ClientAuthService.cs
  modified:
    - server/src/Sven.Abstractions/Configurations/AuthConstants.cs
    - server/src/Sven/Services/DiscoveryService.cs
    - server/src/Sven/Models/TokenRequest.cs

key-decisions:
  - "IClientAuthService is internal (Sven-only) — not in Sven.Abstractions; follows CODE-02 service naming convention"
  - "ClientAuthService checks request.HasFormContentType before accessing Form to avoid InvalidOperationException on non-form requests"

patterns-established:
  - "ClientAuthService reads Authorization header via request.Headers.Authorization (StringValues) for null-safe access"

requirements-completed:
  - PROT-01

# Metrics
duration: 15min
completed: 2026-03-15
---

# Phase 2 Plan 03: Client Auth Prerequisites Summary

**IClientAuthService + ClientAuthService implementing RFC 6749 §2.3 dual-method client authentication with Basic header parsing, Post form extraction, PasswordHasher secret verification, and DiscoveryService expanded for client_credentials grant**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-15T18:10:00Z
- **Completed:** 2026-03-15T18:25:00Z
- **Tasks:** 2
- **Files modified:** 5 (3 modified, 2 created)

## Accomplishments
- Added `AuthConstants.OAuth.GrantTypes.ClientCredentials` constant and removed stale "not implemented" XML comments from `TokenAuthMethods`
- Extended `DiscoveryService` to advertise `client_secret_basic`, `client_secret_post`, and `client_credentials` in discovery document; `TryGetSupportedAuthMethod` now returns `true` for both secret methods
- Removed `[Required]` from `TokenRequest.RedirectUri` (client_credentials has no redirect_uri) and added `ClientSecret` form field property
- Created `IClientAuthService` (internal interface) and `ClientAuthService` (internal sealed class) covering all seven RFC 6749 §2.3 paths: Basic-only, Post-only, dual-method rejection, no-method rejection, unknown client, public client rejection, wrong secret

## Task Commits

Each task was committed atomically:

1. **Task 1: Add ClientCredentials constant and expand DiscoveryService; fix TokenRequest** - `a495f14` (feat)
2. **Task 2: Implement IClientAuthService and ClientAuthService** - `607c82c` (feat)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `server/src/Sven.Abstractions/Configurations/AuthConstants.cs` - Added `GrantTypes.ClientCredentials`; removed "not implemented" XML docs from `TokenAuthMethods.ClientSecretPost/Basic`
- `server/src/Sven/Services/DiscoveryService.cs` - Extended `_grantTypesSupported` and `_tokenEndpointAuthMethodsSupported`; rewrote `TryGetSupportedAuthMethod` to accept both secret methods
- `server/src/Sven/Models/TokenRequest.cs` - Removed `[Required]` from `RedirectUri`; added `ClientSecret` property
- `server/src/Sven/Services/IClientAuthService.cs` - Internal interface: `AuthenticateClientAsync(HttpRequest, CancellationToken) -> Task<Result<Client>>`
- `server/src/Sven/Services/ClientAuthService.cs` - Implementation with Basic extraction (RFC 7617 §2 first-colon split), Post form reading, dual-method check, PasswordHasher verification

## Decisions Made
- `IClientAuthService` kept internal to `Sven` (not added to `Sven.Abstractions`) — it consumes `HttpRequest` which is an ASP.NET Core type, making it unsuitable for Sven.Abstractions (which has no ASP.NET Core dependency)
- `request.HasFormContentType` guard added before `request.Form["client_secret"]` to prevent `InvalidOperationException` on non-form requests

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- dotnet SDK 10.0.200 continues to emit spurious "Question build FAILED" MSBuild artifacts on first incremental build after file edits; actual C# compilation succeeds as confirmed by `dotnet build ... 2>&1 | grep "^Build"` pattern. This is a known SDK 10 artifact documented in 02-01-SUMMARY.md.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `IClientAuthService` / `ClientAuthService` are ready for DI registration in plan 04 (`SvenServiceCollectionExtensions.AddSvenCore`)
- `DiscoveryService` now accepts both secret auth methods — confidential client registration in plan 05 (OidcController) will work correctly
- `TokenRequest.ClientSecret` and removed `[Required]` on `RedirectUri` enable `HandleClientCredentialsFlow` to bind form fields correctly in plan 04
- Unit test stubs in `ClientAuthServiceTests` remain red (Assert.Fail) — they are plan 04 scope once DI wiring is in place

---
*Phase: 02-client-credentials-grant*
*Completed: 2026-03-15*
