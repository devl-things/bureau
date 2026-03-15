---
phase: 01-security-hardening
plan: 04
subsystem: auth
tags: [rate-limiting, asp-net-core, fixed-window, ip-partitioning, integration-tests]

# Dependency graph
requires:
  - phase: 01-security-hardening-01
    provides: auth code exchange and token TTL fixes
  - phase: 01-security-hardening-02
    provides: service layer interfaces (IAuthCodeService, IUserService)
  - phase: 01-security-hardening-03
    provides: AES startup guard, SvenWebAppFactory with Testing environment support
provides:
  - Fixed-window IP-partitioned rate limiting on /connect/token, /connect/authorize, /connect/signin
  - RateLimitingOptions POCO configurable via appsettings.json RateLimiting: section
  - Three SEC-02 integration tests passing (429 + Retry-After)
  - All 9 Phase1 tests green
affects: [future auth endpoints, Sven.Web middleware pipeline]

# Tech tracking
tech-stack:
  added: [System.Threading.RateLimiting (BCL built-in, no NuGet package needed)]
  patterns:
    - IP-partitioned fixed-window rate limiting via RateLimitPartition.GetFixedWindowLimiter
    - OnRejected handler writes Retry-After header from MetadataName.RetryAfter lease metadata
    - EnableRateLimiting attribute applied per-controller and per-PageModel class
    - Low-threshold configuration injected in tests via SvenWebAppFactory.WithExtraConfig

key-files:
  created:
    - server/src/Sven/Configurations/RateLimitingOptions.cs
    - server/src/Sven.Web/Configurations/RateLimiterServiceCollectionExtensions.cs
    - server/tests/Sven.Tests/Security/RateLimitingTests.cs
  modified:
    - server/src/Sven.Web/Program.cs
    - server/src/Sven.Web/Controllers/Connect/TokenController.cs
    - server/src/Sven.Web/Controllers/Connect/AuthorizeController.cs
    - server/src/Sven.Web/Pages/Connect/SignIn.cshtml.cs
    - server/src/Sven/appsettings.json

key-decisions:
  - "IP-based partitioning chosen for rate limiting — client_id partitioning deferred (comment in appsettings.json)"
  - "QueueLimit=0 on all policies — reject immediately, no queuing to prevent latency spikes"
  - "RateLimiterServiceCollectionExtensions placed in Sven.Web (not Sven) — it depends on Microsoft.AspNetCore.RateLimiting which is a web layer concern"
  - "Rate limits configurable in appsettings.json under RateLimiting: section with coded defaults as fallback"

patterns-established:
  - "Rate limit policy names are lowercase kebab: token-endpoint, authorize-endpoint, signin-endpoint"
  - "Integration tests use WithExtraConfig(LowRateLimitConfig) with PermitLimit=2 to trigger 429 with only 3 requests"

requirements-completed: [SEC-02]

# Metrics
duration: ~90min (two sessions combined)
completed: 2026-03-15
---

# Phase 1 Plan 04: SEC-02 Fixed-Window Rate Limiting Summary

**ASP.NET Core built-in fixed-window rate limiting wired on three auth endpoints (token/authorize/signin) with IP partitioning, Retry-After headers, and all 9 Phase1 integration tests passing**

## Performance

- **Duration:** ~90 min (two sessions, second session resumed from partial commit state)
- **Started:** 2026-03-15T11:00:00Z (approximate)
- **Completed:** 2026-03-15T14:30:00Z (approximate)
- **Tasks:** 2 (Task 1: wire middleware + policies; Task 2: turn SEC-02 test stubs green)
- **Files modified:** 8 core files + 64 supporting infrastructure files committed

## Accomplishments

- Three fixed-window rate limiting policies registered via `AddSvenRateLimiter` extension method
- `[EnableRateLimiting]` attributes applied to `TokenController`, `AuthorizeController`, and `SignInModel`
- `app.UseRateLimiter()` placed after `UseRouting()` and before `UseAuthorization()` in Program.cs
- All three SEC-02 integration tests return HTTP 429 with `Retry-After` header under load
- Full Phase1 suite: 9 tests passing, 0 failing

## Task Commits

Each task was committed atomically:

1. **Task 1 + Task 2: Wire rate limiting middleware and turn SEC-02 stubs green** - `8c11f44` (feat)

(Tasks 1 and 2 were combined into a single commit because the implementation was completed in the previous session and found already in place at resume time. The commit also included remaining infrastructure files from Plans 01-01 through 01-03 that were untracked.)

**Prior plan commits that include rate-limiting implementation:**
- `242f410` — fix(01-03): includes RateLimitingOptions.cs, RateLimiterServiceCollectionExtensions.cs, RateLimitingTests.cs, TokenController/AuthorizeController/SignIn.cshtml.cs attributes

## Files Created/Modified

- `server/src/Sven/Configurations/RateLimitingOptions.cs` — POCO with 6 configurable properties (TokenPermitLimit, TokenWindowSeconds, AuthorizePermitLimit, AuthorizeWindowSeconds, SignInPermitLimit, SignInWindowSeconds) with coded defaults
- `server/src/Sven.Web/Configurations/RateLimiterServiceCollectionExtensions.cs` — `AddSvenRateLimiter` extension: three IP-partitioned fixed-window policies, OnRejected writes Retry-After from lease metadata
- `server/src/Sven.Web/Program.cs` — Added `AddSvenRateLimiter` call and `UseRateLimiter()` at correct middleware position
- `server/src/Sven.Web/Controllers/Connect/TokenController.cs` — `[EnableRateLimiting("token-endpoint")]`
- `server/src/Sven.Web/Controllers/Connect/AuthorizeController.cs` — `[EnableRateLimiting("authorize-endpoint")]`
- `server/src/Sven.Web/Pages/Connect/SignIn.cshtml.cs` — `[EnableRateLimiting("signin-endpoint")]`
- `server/src/Sven/appsettings.json` — Added `RateLimiting:` section with production-safe defaults and `_note` key documenting IP partitioning decision
- `server/tests/Sven.Tests/Security/RateLimitingTests.cs` — Three integration tests using `WithExtraConfig(LowRateLimitConfig)` (PermitLimit=2), asserting 429 + Retry-After on third request

## Decisions Made

- IP-based partitioning chosen over client_id partitioning — simpler for initial implementation; `_note` key in appsettings documents the deferred client_id approach
- `QueueLimit = 0` on all policies — reject immediately rather than queue, avoids latency buildup under attack
- Rate limiting extension placed in `Sven.Web` namespace (not `Sven`) — it references `Microsoft.AspNetCore.RateLimiting` which is a web-layer concern; `Sven` is a class library
- `SvenWebAppFactory.WithExtraConfig` pattern used to inject low-threshold config per-test without creating a separate factory class

## Deviations from Plan

None — plan executed exactly as written. All implementation was already in place at session resume (committed in `242f410`). Session completed the commit of remaining infrastructure files from prior plans.

## Issues Encountered

- Session resumed from a state where 5 files were staged but the majority of untracked infrastructure files from Plans 01-01 through 01-03 had not been committed. These were staged and committed in this session alongside the Plan 04 work.
- The implementation was already complete when the session resumed — no new code was required. All 9 Phase1 tests passed immediately.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 1 (Security Hardening) is now complete — all 4 plans executed, all 9 tests green
- The build chain compiles cleanly: Sven.Data → Sven.Data.SqlServer → Sven → Sven.Web → Sven.Tests
- Testing environment fully configured (InMemory EF, dummy credentials, disabled V8/asset pipeline, ValidateOnBuild disabled)
- Rate limiting is production-configurable via appsettings.json — no hardcoded limits

---
*Phase: 01-security-hardening*
*Completed: 2026-03-15*
