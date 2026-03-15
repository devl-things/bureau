---
phase: 02-client-credentials-grant
plan: 01
subsystem: testing
tags: [xunit, client-credentials, tdd, oauth2, oidc]

# Dependency graph
requires:
  - phase: 01-security-hardening
    provides: SvenWebAppFactory, TestDataConstants, Assert.Fail test stub pattern
provides:
  - 19 failing (red) test stubs defining PROT-01 acceptance criteria
  - ClientAuthServiceTests: 6 unit stubs for IClientAuthService
  - Rfc6749ClientCredentialsGrantTests: 9 integration stubs for /connect/token client_credentials grant
  - Rfc7591ConfidentialClientRegistrationTests: 4 integration stubs for /oidc/register confidential client
  - TestDataConstants extended with confidential-client constants
affects:
  - 02-02 (IClientAuthService implementation — will turn unit stubs green)
  - 02-03 (token endpoint client_credentials grant — will turn Rfc6749 stubs green)
  - 02-04 (DCR endpoint — will turn Rfc7591 stubs green)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Wave 0 TDD: write red stubs first, production code in subsequent plans"
    - "IClassFixture<SvenWebAppFactory> for integration tests"
    - "Assert.Fail(\"not implemented\") for all stubs — xUnit 2.5.3 pattern (no SkipException)"
    - "[Trait(\"Category\", \"ClientCredentials\")] for filter-based test selection"

key-files:
  created:
    - server/tests/Sven.Tests/ClientCredentials/ClientAuthServiceTests.cs
    - server/tests/Sven.Tests/ClientCredentials/Rfc6749ClientCredentialsGrantTests.cs
    - server/tests/Sven.Tests/ClientCredentials/Rfc7591ConfidentialClientRegistrationTests.cs
  modified:
    - server/tests/Sven.Tests/TestData/TestDataConstants.cs

key-decisions:
  - "Assert.Fail used for stubs (same pattern as Phase 1) — xUnit 2.5.3 has no SkipException"
  - "Integration test stubs are synchronous void Facts — no async/CancellationToken needed since body is just Assert.Fail"

patterns-established:
  - "ClientCredentials test stubs follow identical Category trait and Assert.Fail body pattern as Phase 1 security stubs"

requirements-completed:
  - PROT-01

# Metrics
duration: 7min
completed: 2026-03-15
---

# Phase 2 Plan 01: Client Credentials Wave 0 Test Stubs Summary

**19 red xUnit stubs defining PROT-01 acceptance criteria across IClientAuthService (unit), /connect/token (integration), and /oidc/register (integration) — zero production code**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-15T17:55:03Z
- **Completed:** 2026-03-15T18:02:22Z
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments
- Extended TestDataConstants with TestConfidentialClientId, TestConfidentialClientSecret, TestIat — shared across all three new test files
- 6 unit test stubs in ClientAuthServiceTests covering Basic auth, Post auth, dual-method rejection, unknown client, wrong secret, and public client rejection
- 13 integration test stubs across Rfc6749 (9 token endpoint behaviors) and Rfc7591 (4 DCR behaviors)
- All 19 stubs compile and fail red with "not implemented" — zero green tests in the ClientCredentials category

## Task Commits

Each task was committed atomically:

1. **Task 1: Add confidential-client constants to TestDataConstants** - `6829470` (test)
2. **Task 2: Create ClientAuthServiceTests unit stubs** - `30b4143` (test)
3. **Task 3: Create Rfc6749 and Rfc7591 integration test stubs** - `e20a314` (test)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `server/tests/Sven.Tests/TestData/TestDataConstants.cs` - Added TestConfidentialClientId, TestConfidentialClientSecret, TestIat
- `server/tests/Sven.Tests/ClientCredentials/ClientAuthServiceTests.cs` - 6 unit stubs for IClientAuthService auth extraction and validation
- `server/tests/Sven.Tests/ClientCredentials/Rfc6749ClientCredentialsGrantTests.cs` - 9 integration stubs for client_credentials grant token endpoint
- `server/tests/Sven.Tests/ClientCredentials/Rfc7591ConfidentialClientRegistrationTests.cs` - 4 integration stubs for DCR confidential client registration

## Decisions Made
- Assert.Fail pattern reused from Phase 1 — xUnit 2.5.3 has no SkipException
- Integration test stubs use synchronous void Facts; no async methods since body is Assert.Fail only

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- dotnet SDK 10.0.200 emits a spurious "Question build" MSBuild message (AssemblyInfoInputs.cache read conflict) on first incremental build attempt; this is a known SDK artifact unrelated to code. Subsequent `--no-restore` builds succeed cleanly.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All PROT-01 test stubs are in place; 02-02 can immediately begin implementing IClientAuthService to turn unit stubs green
- SvenWebAppFactory fixture is referenced by both integration test classes — no factory changes needed for 02-02
- Watson.Items pre-existing build errors (missing EF NuGet references) are unrelated to Sven; full `server/` build targets only Sven-related projects for this feature branch

---
*Phase: 02-client-credentials-grant*
*Completed: 2026-03-15*
