---
phase: 05-bureau-features-registration
plan: 03
subsystem: auth
tags: [oidc, client-registration, bureau-features, ef-core, integration-tests]

# Dependency graph
requires:
  - phase: 05-02
    provides: ClientFeatureDb and ClientFeatureExternalRequirementDb EF entities + migrations
  - phase: 02-client-credentials-grant
    provides: OidcController RegisterAsync, ClientService, ClientRepository patterns
  - phase: 04-end-session-security
    provides: PostLogoutRedirectUris two-column pattern, ClientRegistrationRequest inheritance pattern

provides:
  - bureau_features field constant in AuthConstants.OAuth.FieldNames
  - Dictionary<string, List<string>>? BureauFeatures property on Client and ClientRequest models
  - bureau_features JSON property on both ClientRegistrationRequest types (Sven.Models and Sven.Contracts)
  - ClientService bureau_features validation against FeatureKeys.AllKeys and IExternalProviderRegistry
  - ClientRepository StoreAsync two-save pattern persisting ClientFeatureDb + ClientFeatureExternalRequirementDb rows
  - ClientRepository GetAsync Include chain with eager loading of ClientFeatures and ExternalRequirements
  - ToClient BureauFeatures ToDictionary projection
  - OidcController passing BureauFeatures through ClientRequest and echoing in response
  - 3 passing BureauFeaturesRegistrationTests integration tests (VAULT-01 SC1, SC2, SC3)

affects: [06-token-exchange, future-feature-scoping]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Two-save EF pattern for child rows dependent on parent PK (save parent first to get Id, then save children)
    - Dictionary<string, List<string>> for feature-to-scope mapping across DTOs and domain models
    - ProblemCodes.Request.InvalidPayload for validation errors that should map to HTTP 400

key-files:
  created:
    - server/tests/Sven.Tests/BureauFeatures/BureauFeaturesRegistrationTests.cs (3 passing integration tests)
  modified:
    - server/src/Sven.Abstractions/Configurations/AuthConstants.cs (BureauFeatures constant added to OAuth.FieldNames)
    - server/src/Sven.Abstractions/Models/Client.cs (BureauFeatures property)
    - server/src/Sven.Abstractions/Models/ClientRequest.cs (BureauFeatures property)
    - server/src/Sven/Models/ClientRegistrationRequest.cs (bureau_features JSON property)
    - server/src/Sven.Contracts/ClientRegistrationRequest.cs (bureau_features JSON property)
    - server/src/Sven/Services/ClientService.cs (IExternalProviderRegistry injection + validation block)
    - server/src/Sven/Data/Repositories/ClientRepository.cs (Include chain + ToClient projection + two-save StoreAsync)
    - server/src/Sven.Web/Controllers/OidcController.cs (BureauFeatures passthrough in request and response)

key-decisions:
  - "ProblemCodes.Request.InvalidPayload used for bureau_features validation errors — maps to HTTP 400, consistent with other payload validation in the system"
  - "IExternalProviderRegistry injected into ClientService for scope key validation — registry is a singleton, always available in test environment via AddSvenAuthentication"
  - "RegisteredExternalProvider.ProviderKey (not .Key) used in SelectMany scope enumeration — confirmed from RegisteredExternalProvider record definition"
  - "Two-save EF pattern in StoreAsync: first save persists ClientDb parent to get Id, second save persists ClientFeatureDb children with correct foreign key"
  - "ExternalRequirements assignment uses .ToList() which satisfies ICollection<T> — List<T> implements ICollection<T>"
  - "Test uses public client (token_endpoint_auth_method=none) without IAT — IAT guard only applies to confidential clients in OidcController"

patterns-established:
  - "Parent-first two-save pattern for EF child rows with auto-increment PK dependency"
  - "Bureau features represented as Dictionary<string, List<string>> throughout the stack: registration request, domain model, and DB projection"

requirements-completed: [VAULT-01]

# Metrics
duration: 7min
completed: 2026-03-17
---

# Phase 5 Plan 03: Bureau Features Registration Pipeline Summary

**Full bureau_features pipeline wired from JSON registration request through service validation and EF child-row persistence, with 3 VAULT-01 integration tests passing**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-17T00:47:07Z
- **Completed:** 2026-03-17T00:54:25Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Added `bureau_features` constant and property to all relevant models and DTOs (AuthConstants, Client, ClientRequest, both ClientRegistrationRequest types)
- Implemented ClientService validation checking feature keys against `FeatureKeys.AllKeys` and scope keys against `IExternalProviderRegistry`
- Updated ClientRepository with Include chain for eager loading and two-save StoreAsync for child row persistence
- All 3 BureauFeaturesRegistrationTests integration tests pass (SC1: 201 with features echoed, SC2: 400 on unknown key, SC3: round-trip via IClientService)

## Task Commits

Each task was committed atomically:

1. **Task 1: Models, constants, service validation, and repository round-trip** - `e048566` (feat)
2. **Task 2: Implement integration tests and turn stubs green** - `693a8f3` (feat)

## Files Created/Modified
- `server/src/Sven.Abstractions/Configurations/AuthConstants.cs` - Added BureauFeatures = "bureau_features" constant to OAuth.FieldNames
- `server/src/Sven.Abstractions/Models/Client.cs` - Added Dictionary<string, List<string>>? BureauFeatures property
- `server/src/Sven.Abstractions/Models/ClientRequest.cs` - Added Dictionary<string, List<string>>? BureauFeatures property
- `server/src/Sven/Models/ClientRegistrationRequest.cs` - Added bureau_features JSON property with [FromBody] attribute
- `server/src/Sven.Contracts/ClientRegistrationRequest.cs` - Added bureau_features JSON property with [FromBody] attribute
- `server/src/Sven/Services/ClientService.cs` - IExternalProviderRegistry injection + bureau_features validation block + BureauFeatures assignment
- `server/src/Sven/Data/Repositories/ClientRepository.cs` - Include chain in GetClientDbAsync + BureauFeatures projection in ToClient + two-save StoreAsync
- `server/src/Sven.Web/Controllers/OidcController.cs` - BureauFeatures passed through ClientRequest and response mapping
- `server/tests/Sven.Tests/BureauFeatures/BureauFeaturesRegistrationTests.cs` - 3 async integration tests replacing Assert.Fail stubs

## Decisions Made
- Used `ProblemCodes.Request.InvalidPayload` (HTTP 400) as error code for bureau_features validation — ad-hoc string codes fall back to HTTP 500 in ProblemCodeDefinitionResolver
- Test uses public client (token_endpoint_auth_method=none) — IAT guard only applies to confidential clients, so no Authorization header needed
- `RegisteredExternalProvider.ProviderKey` is the correct property name (not `.Key`) — confirmed from record definition

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Wrong error code causing HTTP 500 instead of HTTP 400 for bureau_features validation**
- **Found during:** Task 2 (integration test implementation and pre-analysis)
- **Issue:** Plan specified `ResultError.From("Unknown bureau feature key", ...)` with ad-hoc string as code; ProblemCodeDefinitionResolver falls back to `system.configuration_error` (HTTP 500) for unrecognised codes
- **Fix:** Changed error code to `ProblemCodes.Request.InvalidPayload` which maps to HTTP 400 in ProblemCodeDefinitionResolver
- **Files modified:** server/src/Sven/Services/ClientService.cs
- **Verification:** SC2 test asserts 400 status and passes
- **Committed in:** `693a8f3` (Task 2 commit)

**2. [Rule 1 - Bug] Wrong property name on RegisteredExternalProvider in scope validation**
- **Found during:** Task 2 (first test run, compile error)
- **Issue:** `p.Key` does not exist on `RegisteredExternalProvider` record — property is `ProviderKey`
- **Fix:** Changed `p.Key` to `p.ProviderKey`
- **Files modified:** server/src/Sven/Services/ClientService.cs
- **Verification:** Compile error resolved, tests pass
- **Committed in:** `693a8f3` (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (2 Rule 1 bugs)
**Impact on plan:** Both fixes essential for correctness. No scope creep.

## Issues Encountered
- `Sven.Abstractions.Services` namespace does not exist — `IClientService` is in `Sven.Services` (files physically live in `Sven.Abstractions/Services/` but namespace is `Sven.Services`). Fixed using directive in test file.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- VAULT-01 complete: bureau_features round-trip from registration to persistence to retrieval
- Ready for Phase 6 (token exchange) to use BureauFeatures when validating scope assignments in token exchange flow

---
*Phase: 05-bureau-features-registration*
*Completed: 2026-03-17*
