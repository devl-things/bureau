---
phase: 03-token-introspection
plan: 01
subsystem: testing
tags: [jwt, xunit, tdd, introspection, rfc7662]

# Dependency graph
requires:
  - phase: 02-client-credentials-grant
    provides: SvenWebAppFactory fixture, SeedConfidentialClientAsync pattern, IClientRepository, SvenTokenProvider, Endpoints.Oidc
provides:
  - Rfc7662TokenIntrospectionTests — 8 Assert.Fail integration test stubs for POST /oidc/introspect
  - TokenProviderIntrospectTests — 5 Assert.Fail unit test stubs for SvenTokenProvider.IntrospectAsync
  - Endpoints.Oidc.Introspect constant (/oidc/introspect)
affects: [03-02, 03-03]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - TDD Wave 0 stub pattern — Assert.Fail bodies with TODO comments showing real assertions
    - [Fact] methods must not have CancellationToken parameters (xUnit 2.5.3 constraint)

key-files:
  created:
    - server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs
    - server/tests/Sven.Tests/Services/TokenProviderIntrospectTests.cs
  modified:
    - server/src/Sven/Configurations/Endpoints.cs

key-decisions:
  - "Endpoints.Oidc.Introspect constant added in Plan 01 (not 02) so integration test file compiles immediately"
  - "xUnit [Fact] methods cannot have CancellationToken parameters — removed from test method signatures"
  - "Unit test stubs (TokenProviderIntrospectTests) compile without ITokenProvider.IntrospectAsync by using Assert.Fail bodies with no production type references"

patterns-established:
  - "Wave 0 TDD: stubs always use Assert.Fail('not implemented') with TODO showing real assertion"

requirements-completed: [PROT-02]

# Metrics
duration: 6min
completed: 2026-03-16
---

# Phase 3 Plan 1: Token Introspection Test Stubs Summary

**13 RFC 7662 introspection test stubs (8 integration + 5 unit) with Assert.Fail bodies establishing the red baseline for Plans 02 and 03**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-16T06:00:11Z
- **Completed:** 2026-03-16T06:06:14Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Created `Rfc7662TokenIntrospectionTests` with 8 integration test stubs covering all PROT-02 endpoint cases
- Created `TokenProviderIntrospectTests` with 5 unit test stubs for `SvenTokenProvider.IntrospectAsync`
- Added `Endpoints.Oidc.Introspect` constant to the shared `Endpoints` configuration class
- Full test suite compiles (`dotnet build` exits 0, 1 pre-existing warning only)

## Task Commits

Each task was committed atomically:

1. **Task 1: Integration test stubs — Rfc7662TokenIntrospectionTests** - `47c4379` (test)
2. **Task 2: Unit test stubs — TokenProviderIntrospectTests** - `4638b48` (test)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs` - 8 integration test stubs for POST /oidc/introspect
- `server/tests/Sven.Tests/Services/TokenProviderIntrospectTests.cs` - 5 unit test stubs for SvenTokenProvider.IntrospectAsync
- `server/src/Sven/Configurations/Endpoints.cs` - Added `Endpoints.Oidc.Introspect = "/oidc/introspect"`

## Decisions Made
- `Endpoints.Oidc.Introspect` added in Plan 01 rather than 02 so the integration test class compiles cleanly against a real constant rather than a string literal.
- xUnit 2.5.3 `[Fact]` methods cannot accept parameters — `CancellationToken cancellationToken = default` removed from test method signatures (the `CancellationToken` convention applies only to non-test async methods).
- Unit stubs (`TokenProviderIntrospectTests`) reference no `ITokenProvider.IntrospectAsync` — pure `Assert.Fail` bodies mean the file compiles today; real assertions dropped in during Plan 02 when the interface method exists.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Removed CancellationToken parameters from [Fact] methods**
- **Found during:** Task 1 (integration test stubs)
- **Issue:** xUnit analyzer error xUnit1001 — Fact methods cannot have parameters; initial stubs included `CancellationToken cancellationToken = default`
- **Fix:** Removed parameter from all 8 test method signatures in Rfc7662TokenIntrospectionTests
- **Files modified:** server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs
- **Verification:** `dotnet build` exits 0 after fix
- **Committed in:** 47c4379 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Minor fix required by xUnit API constraint. No scope change.

## Issues Encountered
- xUnit 2.5.3 rejects `[Fact]` methods with parameters (error xUnit1001) — resolved by removing `CancellationToken` from test signatures. This is the same constraint noted in STATE.md from Phase 1 and 2 stubs.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Red baseline established: 8 integration stubs fail with "not implemented", 5 unit stubs fail with "not implemented"
- Plan 02 (interface + service implementation) can proceed — it will add `ITokenProvider.IntrospectAsync`, `IntrospectionResponse`, and the `/oidc/introspect` endpoint controller
- Unit test stubs will compile against new interface once Plan 02 adds `IntrospectAsync` and removes `Assert.Fail` placeholders

---
*Phase: 03-token-introspection*
*Completed: 2026-03-16*

## Self-Check: PASSED

- FOUND: server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs
- FOUND: server/tests/Sven.Tests/Services/TokenProviderIntrospectTests.cs
- FOUND: .planning/phases/03-token-introspection/03-01-SUMMARY.md
- FOUND commit: 47c4379 (test stubs integration)
- FOUND commit: 4638b48 (test stubs unit)
