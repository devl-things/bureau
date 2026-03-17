---
phase: 05-bureau-features-registration
plan: 02
subsystem: database
tags: [ef-core, postgres, sqlserver, migrations, entity-framework]

requires:
  - phase: 05-01
    provides: stub integration tests (BureauFeaturesRegistrationTests) that this plan's SvenTestContext changes must not break

provides:
  - ClientFeatureDb EF entity with composite PK (ClientId, FeatureKey) and navigation properties
  - ClientFeatureExternalRequirementDb EF entity with three-column composite PK
  - ICollection<ClientFeatureDb> navigation property on ClientDb
  - ClientFeatureBaseTypeConfiguration (provider-neutral base EF config)
  - ClientFeatureExternalRequirementBaseTypeConfiguration (provider-neutral base EF config)
  - Postgres-specific type configurations (text column types)
  - SqlServer-specific type configurations (nvarchar(max) column types)
  - Postgres migration 20260317000001_AddClientFeatures
  - SqlServer migration 20260317000001_AddClientFeatures
  - SvenTestContext registers both new entity types so in-memory tests work

affects: [05-03, 05-bureau-features-registration]

tech-stack:
  added: []
  patterns:
    - "Entity-per-table with composite PKs for child entities — same approach as existing RefreshToken/UserExternalToken tables"
    - "Provider-neutral base config (Sven.Data) + provider-specific override (Sven.Data.Postgres / Sven.Data.SqlServer) separation"
    - "Hand-authored migrations for both providers following existing migration naming convention (YYYYMMDDNNNNNN_Name)"

key-files:
  created:
    - server/src/Sven.Data/Models/ClientFeatureDb.cs
    - server/src/Sven.Data/Models/ClientFeatureExternalRequirementDb.cs
    - server/src/Sven.Data/TypeConfigurations/ClientFeatureBaseTypeConfiguration.cs
    - server/src/Sven.Data/TypeConfigurations/ClientFeatureExternalRequirementBaseTypeConfiguration.cs
    - server/src/Sven.Data.Postgres/TypeConfigurations/ClientFeatureTypeConfiguration.cs
    - server/src/Sven.Data.Postgres/TypeConfigurations/ClientFeatureExternalRequirementTypeConfiguration.cs
    - server/src/Sven.Data.SqlServer/TypeConfigurations/ClientFeatureTypeConfiguration.cs
    - server/src/Sven.Data.SqlServer/TypeConfigurations/ClientFeatureExternalRequirementTypeConfiguration.cs
    - server/src/Sven.Data.Postgres/Migrations/20260317000001_AddClientFeatures.cs
    - server/src/Sven.Data.SqlServer/Migrations/20260317000001_AddClientFeatures.cs
  modified:
    - server/src/Sven.Data/Models/ClientDb.cs
    - server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs

key-decisions:
  - "ClientFeatureDb and ClientFeatureExternalRequirementDb are public (not internal) — consistent with ClientDb which is also public"
  - "Relationship defined once (in ClientFeatureBaseTypeConfiguration from ClientFeatureDb side) — avoids shadow FK columns (RESEARCH pitfall 4)"
  - "Base type configurations omit ToTable() call — provider-specific configs set the table name; in-memory tests do not need it"
  - "nvarchar(max) for SqlServer string columns in composite PK — consistent with prior migrations (Scope, RedirectUris pattern)"

patterns-established:
  - "Child entity config: base config defines PK + relationship; provider configs add column types"
  - "SvenTestContext.OnModelCreating: call Configure() directly for each concrete entity type (ApplyConfigurationsFromAssembly does not work in EF in-memory)"

requirements-completed: [VAULT-01]

duration: 20min
completed: 2026-03-17
---

# Phase 05 Plan 02: Bureau Features Data Layer Summary

**EF entity models, base+provider type configurations, and hand-authored migrations for ClientFeatures and ClientFeatureExternalRequirements tables in both Postgres and SqlServer providers**

## Performance

- **Duration:** ~20 min
- **Started:** 2026-03-17T00:00:00Z
- **Completed:** 2026-03-17T00:20:00Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments

- Created two new EF entity classes with composite PKs establishing the bureau features data model
- Added ICollection<ClientFeatureDb> navigation property to ClientDb linking clients to their features
- Created provider-neutral base type configurations and provider-specific configurations for Postgres (text) and SqlServer (nvarchar(max))
- Authored hand-crafted migrations for both Postgres and SqlServer with correct Up/Down CreateTable/DropTable
- Registered both new entities in SvenTestContext.OnModelCreating — BureauFeatures stub tests run without InvalidOperationException (3 failed via Assert.Fail, 0 errors)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create EF entity models, base type configurations, and update ClientDb** - `34f52d1` (feat)
2. **Task 2: Provider-specific type configurations, migrations, and SvenTestContext update** - `8fbd548` (feat)

**Plan metadata:** (docs commit follows)

## Files Created/Modified

- `server/src/Sven.Data/Models/ClientFeatureDb.cs` - ClientFeatureDb entity, composite PK (ClientId, FeatureKey), navigation to Client and ExternalRequirements
- `server/src/Sven.Data/Models/ClientFeatureExternalRequirementDb.cs` - ClientFeatureExternalRequirementDb entity, three-column composite PK, navigation to ClientFeature
- `server/src/Sven.Data/Models/ClientDb.cs` - Added ICollection<ClientFeatureDb> ClientFeatures navigation property
- `server/src/Sven.Data/TypeConfigurations/ClientFeatureBaseTypeConfiguration.cs` - Provider-neutral EF config for ClientFeatureDb
- `server/src/Sven.Data/TypeConfigurations/ClientFeatureExternalRequirementBaseTypeConfiguration.cs` - Provider-neutral EF config for ClientFeatureExternalRequirementDb
- `server/src/Sven.Data.Postgres/TypeConfigurations/ClientFeatureTypeConfiguration.cs` - Postgres config with text column types
- `server/src/Sven.Data.Postgres/TypeConfigurations/ClientFeatureExternalRequirementTypeConfiguration.cs` - Postgres config with text column types
- `server/src/Sven.Data.SqlServer/TypeConfigurations/ClientFeatureTypeConfiguration.cs` - SqlServer config with nvarchar(max) column types
- `server/src/Sven.Data.SqlServer/TypeConfigurations/ClientFeatureExternalRequirementTypeConfiguration.cs` - SqlServer config with nvarchar(max) column types
- `server/src/Sven.Data.Postgres/Migrations/20260317000001_AddClientFeatures.cs` - Postgres migration creating both tables
- `server/src/Sven.Data.SqlServer/Migrations/20260317000001_AddClientFeatures.cs` - SqlServer migration creating both tables
- `server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs` - SvenTestContext.OnModelCreating now registers ClientFeatureDb and ClientFeatureExternalRequirementDb

## Decisions Made

- `ClientFeatureDb` and `ClientFeatureExternalRequirementDb` are `public` (not `internal`) — consistent with the existing `ClientDb` class which is also `public`. Plan specified `internal` but linter enforced `public` to match the project's existing convention.
- Relationship is defined from the `ClientFeatureDb` side only (in the base configuration) — defining from both sides would create shadow FK columns.
- Base type configurations omit `ToTable()` — EF convention infers the table name; provider-specific configs add it explicitly. In-memory tests work correctly either way.
- `nvarchar(max)` chosen for SqlServer string PK columns — consistent with how existing Scope and RedirectUris columns are stored.

## Deviations from Plan

None — plan executed exactly as written. The linter auto-corrected `internal class` to `public class` on both new entity files, which is consistent with the existing `ClientDb` accessibility level.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Data layer complete: both tables and entities are ready for Plan 03's repository round-trip tests
- SvenTestContext recognizes both new entity types — integration tests will not throw InvalidOperationException during DI startup
- No blockers

---
*Phase: 05-bureau-features-registration*
*Completed: 2026-03-17*

## Self-Check: PASSED

All 11 expected files found. Both task commits (34f52d1, 8fbd548) verified in git log.
