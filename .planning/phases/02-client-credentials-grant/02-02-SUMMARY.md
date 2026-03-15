---
phase: 02-client-credentials-grant
plan: 02
subsystem: database
tags: [efcore, migrations, sqlserver, postgres, client-credentials]

# Dependency graph
requires:
  - phase: 02-client-credentials-grant/02-01
    provides: test stubs for client credentials flow establishing what HashedSecret must support

provides:
  - HashedSecret nullable column on ClientDb entity and Client model
  - EF type configuration with HasMaxLength(200) for HashedSecret
  - ClientRepository round-trips HashedSecret through StoreAsync/ToClient
  - SQL Server migration (20260315000001_AddClientHashedSecret)
  - Postgres migration (20260315000001_AddClientHashedSecret)

affects:
  - 02-03 (ClientAuthService uses HashedSecret for bcrypt verification)
  - 02-04 (TokenController reads authenticated client with HashedSecret populated)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "HashedSecret is a first-class column outside the ClientAddendum JSON blob"
    - "ClientDb in Sven.Data/Models/ is the active EF entity; Sven/Data/Models/ClientDb.cs is excluded from compile"

key-files:
  created:
    - server/src/Sven.Data.SqlServer/Migrations/20260315000001_AddClientHashedSecret.cs
    - server/src/Sven.Data.Postgres/Migrations/20260315000001_AddClientHashedSecret.cs
  modified:
    - server/src/Sven.Abstractions/Models/Client.cs
    - server/src/Sven.Data/Models/ClientDb.cs
    - server/src/Sven.Data/TypeConfigurations/ClientBaseTypeConfiguration.cs
    - server/src/Sven/Data/Repositories/ClientRepository.cs
    - server/src/Sven/Data/Models/ClientDb.cs
    - server/src/Sven.Data.SqlServer/Migrations/SvenContextSqlServerModelSnapshot.cs

key-decisions:
  - "ClientDb in Sven.Data/Models/ is the EF entity used by ClientBaseTypeConfiguration; Sven/Data/Models/ClientDb.cs is excluded from Sven.csproj compile via <Compile Remove=Data\\Models\\**>"
  - "Sven.Data.Postgres had pre-existing build errors (accessibility violations in TypeConfigurations) unrelated to this plan — migration file created correctly, postgres project failures deferred"

patterns-established:
  - "First-class columns added directly to ClientDb and ClientBaseTypeConfiguration, never via ClientAddendum JSON"

requirements-completed:
  - PROT-01

# Metrics
duration: 10min
completed: 2026-03-15
---

# Phase 02 Plan 02: HashedSecret Column — Schema Foundation Summary

**HashedSecret added as a first-class nullable nvarchar(200)/varchar(200) column on Clients table, round-tripped through ClientRepository, with SQL Server and Postgres migrations**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-03-15T18:00:00Z
- **Completed:** 2026-03-15T18:09:59Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- `string? HashedSecret` added to `Client` (Sven.Abstractions) and `ClientDb` (Sven.Data) — kept outside `ClientAddendum` JSON
- `ClientBaseTypeConfiguration` configures `HasMaxLength(200)` on `HashedSecret`
- `ClientRepository.StoreAsync` persists `HashedSecret`; `ToClient` maps it back — full round-trip confirmed
- SQL Server migration `20260315000001_AddClientHashedSecret` adds nullable `nvarchar(200)` column
- Postgres migration `20260315000001_AddClientHashedSecret` adds nullable `character varying(200)` column
- Model snapshot updated with `HashedSecret` property

## Task Commits

1. **Task 1: Add HashedSecret to Client model and ClientDb entity** - `69d0a9c` (feat)
2. **Task 2: Wire HashedSecret through ClientRepository and add EF migrations** - `e43b15d` (feat)

**Plan metadata:** (docs commit — see below)

## Files Created/Modified

- `server/src/Sven.Abstractions/Models/Client.cs` — added `string? HashedSecret` property; removed stale `#38` comment
- `server/src/Sven.Data/Models/ClientDb.cs` — added `string? HashedSecret` (active EF entity)
- `server/src/Sven/Data/Models/ClientDb.cs` — added `string? HashedSecret` (excluded from compile, kept in sync)
- `server/src/Sven.Data/TypeConfigurations/ClientBaseTypeConfiguration.cs` — `HasMaxLength(200)` on HashedSecret
- `server/src/Sven/Data/Repositories/ClientRepository.cs` — StoreAsync and ToClient wired for HashedSecret
- `server/src/Sven.Data.SqlServer/Migrations/20260315000001_AddClientHashedSecret.cs` — created
- `server/src/Sven.Data.SqlServer/Migrations/SvenContextSqlServerModelSnapshot.cs` — HashedSecret added
- `server/src/Sven.Data.Postgres/Migrations/20260315000001_AddClientHashedSecret.cs` — created

## Decisions Made

- `Sven.Data/Models/ClientDb.cs` is the EF entity; `Sven/Data/Models/ClientDb.cs` is excluded from `Sven.csproj` via `<Compile Remove="Data\Models\**" />`. Both were updated to stay in sync.
- `Sven.Data.Postgres` had pre-existing build errors in TypeConfigurations (accessibility violations for `HouseholdDb`, `RefreshTokenDb`, etc.) unrelated to this plan. These were present before any changes. The postgres migration file itself is correct; the broader project errors are deferred.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Added HashedSecret to both ClientDb files**
- **Found during:** Task 1 (compilation)
- **Issue:** Plan specified `Sven/Data/Models/ClientDb.cs` but `Sven.csproj` excludes that file from compile. `ClientBaseTypeConfiguration` in `Sven.Data` references `Sven.Data/Models/ClientDb.cs`. Adding HashedSecret only to the `Sven/` version caused CS1061 error.
- **Fix:** Added `HashedSecret` to `Sven.Data/Models/ClientDb.cs` (the active entity). Also updated `Sven/Data/Models/ClientDb.cs` to keep it in sync.
- **Files modified:** `server/src/Sven.Data/Models/ClientDb.cs`, `server/src/Sven/Data/Models/ClientDb.cs`
- **Verification:** `dotnet build server/src/Sven/Sven.csproj --no-restore` — Build succeeded
- **Committed in:** `69d0a9c` (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - Bug)
**Impact on plan:** Required for correctness — compile would fail otherwise. No scope creep.

## Issues Encountered

- `Sven.Data.Postgres` has pre-existing build failures (TypeConfiguration accessibility errors) that predate this plan. Logged to deferred-items. The migration file content is correct.
- `dotnet build` with multiple csproj paths fails (MSBuild only accepts one project per invocation). Built each project individually.

## User Setup Required

None — schema changes are applied via EF migrations at deploy/startup time.

## Next Phase Readiness

- `HashedSecret` column exists in schema model and EF entity
- Repository persists and retrieves `HashedSecret` correctly
- Plan 03 (`ClientAuthService`) can now implement bcrypt verification against `client.HashedSecret`

---
*Phase: 02-client-credentials-grant*
*Completed: 2026-03-15*
