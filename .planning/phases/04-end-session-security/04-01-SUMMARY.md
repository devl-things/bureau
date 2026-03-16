---
phase: 04-end-session-security
plan: 01
subsystem: testing
tags: [xunit, integration-tests, oidc, end-session, rp-initiated-logout]

# Dependency graph
requires:
  - phase: 01-security-hardening
    provides: SvenWebAppFactory test fixture and Assert.Fail stub pattern
provides:
  - Four failing RpInitiatedLogoutTests stubs targeting all PROT-04 logout scenarios
affects:
  - 04-02 (implements EndSession controller hardening — these stubs are its verification targets)
  - 04-03 (PostLogoutRedirectUris schema — SC2/SC3 stubs verify redirect behavior)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Assert.Fail stubs — synchronous void [Fact] methods with Assert.Fail body for pre-production test stubs (xUnit 2.5.3 pattern)"
    - "AllowAutoRedirect=false on HttpClient — required when tests verify 302 vs 200 directly"

key-files:
  created:
    - server/tests/Sven.Tests/EndSession/RpInitiatedLogoutTests.cs
  modified: []

key-decisions:
  - "Synchronous void [Fact] methods used (not async) — bodies are Assert.Fail only, no awaitable operations needed; consistent with Phase 1/2/3 stub pattern"
  - "AllowAutoRedirect=false on WebApplicationFactoryClientOptions — tests will verify 302 Location header directly, not follow the redirect"

patterns-established:
  - "EndSession test category: [Trait(\"Category\", \"EndSession\")] for filter --filter Category=EndSession"

requirements-completed:
  - PROT-04

# Metrics
duration: 8min
completed: 2026-03-16
---

# Phase 4 Plan 01: End Session Test Stubs Summary

**Four synchronous Assert.Fail stubs covering all PROT-04 RP-initiated logout scenarios (invalid hint, unregistered redirect, registered redirect, no redirect)**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-16T10:05:00Z
- **Completed:** 2026-03-16T10:13:00Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- Created `RpInitiatedLogoutTests.cs` in new `EndSession/` test directory
- Four failing test stubs with `Category=EndSession` trait for filter targeting
- IClassFixture<SvenWebAppFactory> with `AllowAutoRedirect=false` — ready for redirect-checking assertions in Wave 1/2 plans
- All four tests fail with "not implemented" as expected; zero passing, zero compile errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create RpInitiatedLogoutTests.cs with four Assert.Fail stubs** - `40fa31c` (test)

**Plan metadata:** (docs commit follows)

## Files Created/Modified
- `server/tests/Sven.Tests/EndSession/RpInitiatedLogoutTests.cs` - Four Assert.Fail stubs for PROT-04 SC1–SC4

## Decisions Made
- Synchronous void `[Fact]` methods (not async) — consistent with xUnit 2.5.3 stub pattern established in Phase 1/2/3; Assert.Fail bodies need no async
- `AllowAutoRedirect = false` on HttpClient — SC3 test will verify 302 + Location header directly, not follow the redirect

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Stale `Bureau.Primitives` AssemblyInfoInputs.cache caused initial `--no-restore` build failure; deleted obj/ directory to clear it. Build succeeded on retry. This is a pre-existing environment issue unrelated to the new file.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All four PROT-04 test stubs are live and failing with "not implemented"
- Plan 04-02 can begin: implement `EndSessionController` id_token_hint full JWT signature validation (SC1 + SC4 go green)
- Plan 04-03 follows: add `PostLogoutRedirectUris` schema + redirect URI validation (SC2 + SC3 go green)

---
*Phase: 04-end-session-security*
*Completed: 2026-03-16*
