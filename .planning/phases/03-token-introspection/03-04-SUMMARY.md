---
phase: 03-token-introspection
plan: 04
subsystem: testing
tags: [jwt, introspection, rfc7662, xunit, integration-test]

# Dependency graph
requires:
  - phase: 03-token-introspection-03
    provides: SvenTokenProvider.IntrospectAsync with ValidateLifetime=true and full signature validation
provides:
  - ExpiredToken_Returns200_WithActiveFalse_NoExtraClaims fully implemented with real assertion (no Assert.Fail)
  - PROT-02 Success Criterion 2 backed by integration-test evidence
affects: [03-token-introspection, PROT-02 closure]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Server key retrieval in integration tests: _factory.Services.GetRequiredService<RsaSecurityKey>() obtains the singleton key used to sign real tokens"
    - "Expired JWT construction: JwtSecurityToken with expires = DateTime.UtcNow.AddHours(-1) signed with server key triggers ValidateLifetime=true rejection"

key-files:
  created: []
  modified:
    - server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs

key-decisions:
  - "Expired token integration test needs no time-travel: manually construct JWT with exp in the past signed with the real server key; ValidateLifetime=true in TokenValidationParameters rejects it at introspect time"

patterns-established:
  - "Integration test expired-token pattern: resolve server RsaSecurityKey from DI, construct JwtSecurityToken with past exp, sign with server key to pass signature check but fail lifetime check"

requirements-completed: [PROT-02]

# Metrics
duration: 8min
completed: 2026-03-16
---

# Phase 3 Plan 04: Token Introspection — Expired Token Gap Closure Summary

**Server-signed expired JWT integration test proves `active:false` with no extra claims, closing PROT-02 Success Criterion 2 without any time-travel mock.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-16T06:45:00Z
- **Completed:** 2026-03-16T06:53:00Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments

- Replaced the permanently-deferred `Assert.Fail` stub in `ExpiredToken_Returns200_WithActiveFalse_NoExtraClaims` with a real integration test
- Test resolves the server's `RsaSecurityKey` singleton from DI so signature validation passes, but sets `exp = DateTime.UtcNow.AddHours(-1)` so `ValidateLifetime=true` rejects the token
- Asserts HTTP 200, `active:false`, and absence of all 7 extra claim fields (`sub`, `scope`, `exp`, `iat`, `jti`, `iss`, `client_id`)
- All 8 tests in `Rfc7662TokenIntrospectionTests` pass

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement ExpiredToken integration test** - `9365ea7` (feat)

## Files Created/Modified

- `server/tests/Sven.Tests/TokenIntrospection/Rfc7662TokenIntrospectionTests.cs` - Replaced Assert.Fail stub with real expired-token integration test

## Decisions Made

- No time-travel mechanism (TimeProvider, mocking) is needed: a manually constructed JWT with a past `exp` value, signed with the real server key, exercises the actual `ValidateLifetime=true` code path in `SvenTokenProvider.IntrospectAsync`.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

The first test run used `--no-build` which referenced a cached DLL. Removed the flag on the second run to pick up the freshly compiled binary. Both builds succeeded cleanly.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- PROT-02 now has both unit-test evidence (TokenProviderIntrospectTests) and integration-test evidence (Rfc7662TokenIntrospectionTests) for the expired-token case
- Phase 3 token introspection plans are complete; no blockers for subsequent phases

---
*Phase: 03-token-introspection*
*Completed: 2026-03-16*
