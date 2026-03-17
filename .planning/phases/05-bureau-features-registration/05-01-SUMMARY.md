---
phase: 05-bureau-features-registration
plan: 01
subsystem: testing
tags: [xunit, integration-tests, tdd, bureau-features]

# Dependency graph
requires:
  - phase: 04-end-session-security
    provides: SvenWebAppFactory pattern and IClassFixture<SvenWebAppFactory> established
provides:
  - Three Assert.Fail stub tests for VAULT-01 SC1/SC2/SC3 in BureauFeaturesRegistrationTests
affects: [05-02-bureau-features-data-layer, 05-03-bureau-features-endpoint]

# Tech tracking
tech-stack:
  added: []
  patterns: [Assert.Fail stubs for TDD RED phase, xUnit IClassFixture<SvenWebAppFactory> with IDisposable]

key-files:
  created:
    - server/tests/Sven.Tests/BureauFeatures/BureauFeaturesRegistrationTests.cs
  modified:
    - server/src/Sven.Data/Models/ClientFeatureDb.cs
    - server/src/Sven.Data/Models/ClientFeatureExternalRequirementDb.cs
    - server/src/Sven.Data/TypeConfigurations/ClientFeatureBaseTypeConfiguration.cs
    - server/src/Sven.Data/TypeConfigurations/ClientFeatureExternalRequirementBaseTypeConfiguration.cs

key-decisions:
  - "BureauFeaturesRegistrationTests uses synchronous void [Fact] stubs (Assert.Fail) — xUnit 2.5.3 pattern, same as prior phases"
  - "ClientFeatureDb and ClientFeatureExternalRequirementDb changed from internal to public — required by public navigation property on public ClientDb"
  - "ToTable() calls removed from base type configurations in Sven.Data — relational extension not available in EF Core base package; convention naming used instead"

patterns-established:
  - "BureauFeatures test category: [Trait(Category, BureauFeatures)] on class"

requirements-completed: [VAULT-01]

# Metrics
duration: 5min
completed: 2026-03-17
---

# Phase 5 Plan 01: Bureau Features Registration Test Stubs Summary

**Three Assert.Fail stub tests for VAULT-01 success criteria establish RED baseline for bureau_features registration endpoint**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-17T13:20:22Z
- **Completed:** 2026-03-17T13:24:47Z
- **Tasks:** 1
- **Files modified:** 5

## Accomplishments
- Created `BureauFeaturesRegistrationTests` with 3 stub methods mapping to VAULT-01 SC1 (201 with features), SC2 (400 on unknown key), SC3 (GET retrieves features)
- Fixed blocking build errors in Sven.Data from Phase 5 planning stubs (accessibility + relational extension)
- Full build passes; `dotnet test --filter Category=BureauFeatures` reports exactly 3 failed, 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create BureauFeaturesRegistrationTests stubs** - `bf40ea1` (test)

**Plan metadata:** (docs commit below)

## Files Created/Modified
- `server/tests/Sven.Tests/BureauFeatures/BureauFeaturesRegistrationTests.cs` - Three Assert.Fail stubs with [Trait(Category, BureauFeatures)], IClassFixture<SvenWebAppFactory>, IDisposable
- `server/src/Sven.Data/Models/ClientFeatureDb.cs` - Changed `internal` to `public` (accessibility fix)
- `server/src/Sven.Data/Models/ClientFeatureExternalRequirementDb.cs` - Changed `internal` to `public` (accessibility fix)
- `server/src/Sven.Data/TypeConfigurations/ClientFeatureBaseTypeConfiguration.cs` - Removed `ToTable("ClientFeatures")` call
- `server/src/Sven.Data/TypeConfigurations/ClientFeatureExternalRequirementBaseTypeConfiguration.cs` - Removed `ToTable("ClientFeatureExternalRequirements")` call

## Decisions Made
- Assert.Fail used (not SkipException) — xUnit 2.5.3 does not expose SkipException; same pattern as all prior phases
- Test stubs are synchronous void [Fact] — no async needed since body is Assert.Fail only

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed internal accessibility on ClientFeatureDb and ClientFeatureExternalRequirementDb**
- **Found during:** Task 1 (build step before test run)
- **Issue:** `ClientDb.ClientFeatures` is a public property on a public class referencing `ICollection<ClientFeatureDb>`, but `ClientFeatureDb` was declared `internal` — CS0053 inconsistent accessibility
- **Fix:** Changed both `ClientFeatureDb` and `ClientFeatureExternalRequirementDb` from `internal` to `public`
- **Files modified:** server/src/Sven.Data/Models/ClientFeatureDb.cs, server/src/Sven.Data/Models/ClientFeatureExternalRequirementDb.cs
- **Verification:** Build succeeded after change
- **Committed in:** bf40ea1 (part of task commit)

**2. [Rule 3 - Blocking] Removed ToTable() from base type configurations**
- **Found during:** Task 1 (build step)
- **Issue:** `ClientFeatureBaseTypeConfiguration` and `ClientFeatureExternalRequirementBaseTypeConfiguration` called `builder.ToTable()`, which is a relational extension — not available in `Microsoft.EntityFrameworkCore` (base package only) referenced by Sven.Data
- **Fix:** Removed both `ToTable()` calls; EF convention naming applies (tables named `ClientFeatures` and `ClientFeatureExternalRequirements`)
- **Files modified:** server/src/Sven.Data/TypeConfigurations/ClientFeatureBaseTypeConfiguration.cs, server/src/Sven.Data/TypeConfigurations/ClientFeatureExternalRequirementBaseTypeConfiguration.cs
- **Verification:** Build succeeded after change
- **Committed in:** bf40ea1 (part of task commit)

---

**Total deviations:** 2 auto-fixed (2 blocking pre-existing build errors from Phase 5 planning stubs)
**Impact on plan:** Both fixes required for compilation. No scope creep — changes confined to files introduced by Phase 5 planning prep.

## Issues Encountered
None beyond the auto-fixed blocking build issues above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- RED baseline established: 3 tests in BureauFeatures category all fail with Assert.Fail stubs
- Plan 02 (data layer) and Plan 03 (endpoint) can now target these exact test names
- Build is clean; no blocking issues remaining in Sven.Data base configurations

---
*Phase: 05-bureau-features-registration*
*Completed: 2026-03-17*
