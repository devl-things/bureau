---
phase: 04-end-session-security
plan: 02
subsystem: database
tags: [ef-core, migrations, postgres, sqlserver, domain-model]

# Dependency graph
requires:
  - phase: 04-end-session-security
    provides: "EndSession test stubs (Assert.Fail) defining PROT-04 data contract requirements"
provides:
  - "PostLogoutRedirectUris as List<string>? on Client domain model"
  - "PostLogoutRedirectUris as List<string>? JSON column on ClientDb EF entity"
  - "JSON conversion registered in ClientBaseTypeConfiguration"
  - "ClientRepository.ToClient maps PostLogoutRedirectUris from ClientDb to Client"
  - "ClientRepository.StoreAsync persists PostLogoutRedirectUris from Client to ClientDb"
  - "Hand-authored EF migrations for Postgres (text) and SqlServer (nvarchar(max))"
affects:
  - "04-end-session-security (plans 03+: EndSessionController, OidcController, registration pipeline)"

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Hand-authored EF migrations (no dotnet ef tooling) following 20260315000001_AddClientHashedSecret pattern"
    - "Nullable JSON column for optional List<string>? fields — matches existing Contacts pattern"

key-files:
  created:
    - server/src/Sven.Data.Postgres/Migrations/20260316000001_AddPostLogoutRedirectUris.cs
    - server/src/Sven.Data.SqlServer/Migrations/20260316000001_AddPostLogoutRedirectUris.cs
  modified:
    - server/src/Sven.Abstractions/Models/Client.cs
    - server/src/Sven.Data/Models/ClientDb.cs
    - server/src/Sven.Data/TypeConfigurations/ClientBaseTypeConfiguration.cs
    - server/src/Sven/Data/Repositories/ClientRepository.cs

key-decisions:
  - "PostLogoutRedirectUris stored as nullable JSON text column (text / nvarchar(max)) — same pattern as Contacts, not as structured relational data"
  - "Postgres migration uses 'text' type for unbounded List<string> JSON; SqlServer uses 'nvarchar(max)' — matches existing RedirectUris/Contacts pattern"

patterns-established:
  - "New nullable JSON list columns: add doc comment '/// <summary> as json </summary>' on ClientDb property"
  - "New JSON columns require ConfigureJsonConversion call in ClientBaseTypeConfiguration.Configure()"
  - "Repository round-trip: ToClient reads nullable list directly; StoreAsync assigns nullable list directly"

requirements-completed: [PROT-04]

# Metrics
duration: 3min
completed: 2026-03-16
---

# Phase 4 Plan 2: PostLogoutRedirectUris Data Layer Summary

**PostLogoutRedirectUris added as nullable JSON text column to Client domain model, ClientDb EF entity, and both provider migrations, with full repository round-trip mapping**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-16T10:41:18Z
- **Completed:** 2026-03-16T10:44:11Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- Added `List<string>? PostLogoutRedirectUris` to `Client` domain model (Sven.Abstractions)
- Added `List<string>? PostLogoutRedirectUris` JSON column to `ClientDb` EF entity with JSON conversion registered in `ClientBaseTypeConfiguration`
- Hand-authored migrations for both providers: Postgres (`text`, nullable) and SqlServer (`nvarchar(max)`, nullable)
- Wired `ClientRepository.ToClient` and `StoreAsync` for full persistence round-trip
- All four EndSession test stubs remain failing with Assert.Fail (expected — not implemented yet); no pre-existing tests regressed

## Task Commits

Each task was committed atomically:

1. **Task 1: Add PostLogoutRedirectUris to domain model and EF entity + config** - `1d80a62` (feat)
2. **Task 2: Update ClientRepository mapping + create hand-authored EF migrations** - `37846be` (feat)

**Plan metadata:** (docs commit below)

## Files Created/Modified
- `server/src/Sven.Abstractions/Models/Client.cs` - Added `List<string>? PostLogoutRedirectUris` property after `RedirectUris`
- `server/src/Sven.Data/Models/ClientDb.cs` - Added `List<string>? PostLogoutRedirectUris` with `/// <summary> as json </summary>` doc comment
- `server/src/Sven.Data/TypeConfigurations/ClientBaseTypeConfiguration.cs` - Added `ConfigureJsonConversion` call for `PostLogoutRedirectUris`
- `server/src/Sven/Data/Repositories/ClientRepository.cs` - Mapped `PostLogoutRedirectUris` in both `ToClient` and `StoreAsync`
- `server/src/Sven.Data.Postgres/Migrations/20260316000001_AddPostLogoutRedirectUris.cs` - Postgres migration (`text`, nullable)
- `server/src/Sven.Data.SqlServer/Migrations/20260316000001_AddPostLogoutRedirectUris.cs` - SqlServer migration (`nvarchar(max)`, nullable)

## Decisions Made
- `PostLogoutRedirectUris` stored as nullable JSON text column — same pattern as existing `Contacts` property (not structured relational rows), appropriate for a small list that is read/written atomically with the client record.
- Postgres migration uses `"text"` type (unbounded), SqlServer uses `"nvarchar(max)"` — consistent with how `RedirectUris` and `Contacts` columns are typed in existing migrations.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- `Sven.Data.Postgres` project has pre-existing CS0122/CS0246 build failures in `TypeConfigurations/` (HouseholdDb, RefreshTokenDb etc. protection level and missing Bureau.EntityFrameworkCore namespace) — unrelated to migration files added in this plan. Verified migration file syntax correct; SqlServer provider built with 0 errors.

## User Setup Required
None - no external service configuration required. Migrations are hand-authored and applied at runtime.

## Next Phase Readiness
- `Client.PostLogoutRedirectUris` data contract is in place; all downstream plans (EndSessionController, `post_logout_redirect_uri` validation in OidcController, client registration pipeline) can now reference this property
- No blockers for plan 04-03

---
*Phase: 04-end-session-security*
*Completed: 2026-03-16*
