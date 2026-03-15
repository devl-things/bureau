---
phase: 01-security-hardening
plan: 05
subsystem: Sven (Auth Code Exchange / Rate Limiting)
tags: [security, atomicity, rate-limiting, middleware-pipeline]
dependency_graph:
  requires: []
  provides: [atomic-auth-code-exchange, production-rate-limit-config, correct-middleware-order]
  affects: [AuthCodeService, Program.cs, appsettings.json]
tech_stack:
  added: []
  patterns: [ConcurrentDictionary.TryRemove atomic pattern, ASP.NET Core middleware ordering]
key_files:
  created: []
  modified:
    - server/src/Sven/Services/AuthCodeService.cs
    - server/src/Sven/appsettings.json
    - server/src/Sven.Web/Program.cs
decisions:
  - "AuthCodeService constructor now accepts InMemoryStore<string, AuthCode> directly; IStore<string, AuthCode> and InMemoryStore<string, AuthCode> both registered as singletons sharing the same instance via DI factory"
  - "GetAuthCodeAsync and ClearAuthCodeAsync retained on IAuthCodeService — still used by TokenController's two-step flow, which is out of scope for this plan"
  - "MapRazorPages and MapControllers moved after UseRateLimiter + UseAuthorization; Swagger block remains at development-only position before UseHttpsRedirection"
metrics:
  duration_minutes: 15
  completed_date: "2026-03-15"
  tasks_completed: 2
  files_modified: 3
---

# Phase 1 Plan 05: SEC-02/SEC-03 Gap Closure Summary

**One-liner:** Closed auth-code race window with atomic TryRemoveAtomic, added production RateLimiting defaults to appsettings.json, and moved endpoint mapping after UseRateLimiter so rate-limit policies apply to Razor Pages.

## What Was Built

This plan closed the gaps identified in the VERIFICATION.md report for SEC-02 and SEC-03:

1. **Atomic auth code exchange (SEC-03):** `ExchangeCodeAsync` previously used a non-atomic `GetAsync` + `RemoveAsync` two-step with a race window between code lookup and deletion. The method now calls `TryRemoveAtomic` (a direct `ConcurrentDictionary.TryRemove` wrapper on `InMemoryStore`) — a single lock-free atomic operation with no race window.

2. **Production RateLimiting defaults (SEC-02):** The `RateLimiting` section was absent from `appsettings.json`, meaning production deployments had no default thresholds. Six production-safe defaults were added: 10 req/60s for the token endpoint, 20 req/60s for the authorize endpoint, and 10 req/300s for the sign-in page.

3. **Middleware pipeline order fix (SEC-02):** `MapRazorPages()` was called before `UseRateLimiter()`, which meant the rate-limit policy assigned to the sign-in Razor Page could not intercept those requests. Both `MapRazorPages()` and `MapControllers()` were moved to after `UseRateLimiter()` and `UseAuthorization()`.

## Verification Results

All post-task checks passed:

1. `grep TryRemoveAtomic AuthCodeService.cs` — found at line 93
2. No `GetAsync`/`RemoveAsync` calls within `ExchangeCodeAsync` body
3. All 6 RateLimiting keys present in `appsettings.json`
4. `UseRateLimiter` (line 188) before `MapRazorPages` (line 192) — ORDER OK

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 3af83f7 | fix(01-05): make ExchangeCodeAsync atomic via TryRemoveAtomic |
| 2 | 4d4c2f8 | fix(01-05): add RateLimiting config defaults and fix middleware order |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing critical functionality] DI registration of InMemoryStore<string, AuthCode> as concrete type**
- **Found during:** Task 1
- **Issue:** Program.cs only registered `InMemoryStore<string, AuthCode>` mapped to `IStore<string, AuthCode>`. Changing the `AuthCodeService` constructor parameter to `InMemoryStore<string, AuthCode>` required the concrete type also be resolvable from DI.
- **Fix:** Added `AddSingleton<InMemoryStore<string, AuthCode>>()` and changed the `IStore<string, AuthCode>` registration to a factory that resolves from the concrete singleton, ensuring both types share the same instance.
- **Files modified:** `server/src/Sven.Web/Program.cs`
- **Commit:** 3af83f7

## Self-Check: PASSED
