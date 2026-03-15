# Structure

## Top-Level Layout

```
bureau/
  server/                     — .NET solution (Bureau.slnx)
    src/
      Bureau.AspNetCore/      — Shared ASP.NET Core infrastructure
      Bureau.EntityFrameworkCore/        — Shared EF type conversions
      Bureau.EntityFrameworkCore.SqlServer/
      Bureau.Extensions.Logging/
      Bureau.Extensions.Time/
      Bureau.Frontend.DevRunner/         — Dev runner (starts .NET + pnpm dev)
      Bureau.Primitives/                 — Result<T>, FeatureKeys, ProblemCodes, paging
      Bureau.Server.Contracts/           — Shared route constants, cross-service mappers
      Bureau.Server.Hosting/             — JWT auth setup, dev auth handlers
      Niles.Chores/                      — Chores domain logic + EF data
      Niles.Chores.Abstractions/         — Chores interfaces + models
      Niles.Chores.Api/                  — Chores HTTP API (port 5300)
      Niles.Chores.Contracts/            — Chores API routes + DTOs
      Sven/                              — OIDC core services (legacy, migrating to Sven.Web)
      Sven.Abstractions/                 — Sven interfaces + models
      Sven.Contracts/                    — Sven API contract types
      Sven.Data.Postgres/               — Sven Postgres EF provider
      Sven.Data.SqlServer/              — Sven SQL Server EF provider
      Sven.Web/                          — Sven OIDC host (Razor Pages + Controllers)
      Watson.Items/                      — Items domain models
      Watson.Items.Ingest/               — ETL pipeline logic
      Watson.Items.Ingest.Api/           — Items ingest HTTP API (port 5210)
      Watson.Nodes/                      — Nodes domain logic + EF data
      Watson.Nodes.Abstractions/         — Nodes interfaces, models, conventions
      Watson.Nodes.Api/                  — Nodes HTTP API (port 5200)
      Watson.Nodes.Contracts/            — Nodes API routes + DTOs
      Watson.Nodes.Data.SqlServer/       — Nodes SQL Server EF provider
    tests/
      Sven.Tests/                        — Sven integration + unit tests
  app/                        — pnpm workspace
    client/
      apps/
        bureau-web/           — Main bureau SPA (React + Vite, port 5000)
        niles-chores-web/     — Standalone chores SPA
        playground/           — Component playground
      libs/
        bureau-shell/         — Shell layout, auth context, theme
        client-core/          — Shared utilities, API clients
        chores-admin/         — Chores admin feature module
        nodes-admin/          — Nodes admin feature module
  docs/                       — Architecture documentation
    VISION.md
    DEPLOYMENT.md
    AUTH.md
    FRONTEND-ARCHITECTURE.md
    SVEN.md
  .planning/                  — GSD planning files
    codebase/                 — This codebase map
```

## Domain Project Layout Pattern

Each domain follows this consistent structure:

```
[Domain].Api/
  Controllers/          — MVC controllers (inherit BureauApiControllerBase)
  Dtos/                 — Request/response DTOs
  Factories/            — Query/command factories from DTOs
  Mappers/              — Extension methods: Domain → DTO
  Program.cs

[Domain] (logic + data layer):
  Configurations/       — ServiceCollectionExtensions, DI registration
  Contexts/             — EF DbContext
  Migrations/           — EF migrations
  Models/               — EF entity models (may differ from domain models)
  Services/             — IService implementations
  TypeConfigurations/   — EF Fluent API entity configurations
  Mappers/              — DB entity → domain model mappers

[Domain].Abstractions/
  Models/               — Domain model classes/records
  Services/             — IService interfaces
  Data/                 — IRepository/IStore interfaces (where applicable)

[Domain].Contracts/
  — API route constants (ApiRoutes), shared DTO types used across API boundary
```

## Key File Locations

| Purpose | Path |
|---|---|
| Shared error types | `server/src/Bureau.Primitives/Errors/ProblemCodes.cs` |
| Feature flag keys | `server/src/Bureau.Primitives/Features/FeatureKeys.cs` |
| Result type | `server/src/Bureau.Primitives/Models/Result.cs` |
| Base controller | `server/src/Bureau.AspNetCore/Controllers/BureauApiControllerBase.cs` |
| RequireFeature attr | `server/src/Bureau.AspNetCore/Features/RequireFeatureAttribute.cs` |
| CORS config | `server/src/Bureau.AspNetCore/Cors/` |
| JWT auth setup | `server/src/Bureau.Server.Hosting/Configurations/AuthServiceCollectionExtensions.cs` |
| Dev auth handler | `server/src/Bureau.Server.Hosting/Dev/DevApiTokenAuthenticationHandler.cs` |
| Chores API entry | `server/src/Niles.Chores.Api/Program.cs` |
| Nodes API entry | `server/src/Watson.Nodes.Api/Program.cs` |
| Items ingest entry | `server/src/Watson.Items.Ingest.Api/Program.cs` |
| Sven OIDC entry | `server/src/Sven.Web/Program.cs` |
| Node kind handlers | `server/src/Watson.Nodes/Services/` |
| Frontend config | `app/client/apps/bureau-web/public/config.json` (gitignored) |
| Frontend feature keys | `app/client/apps/bureau-web/src/features/featureKeys.ts` |
| Shell layout | `app/client/libs/bureau-shell/src/` |
| Dev runner | `server/src/Bureau.Frontend.DevRunner/Program.cs` |
| Solution file | `server/Bureau.slnx` |
| Build props | `server/src/Directory.Build.props` |

## Naming Conventions

### Backend
- Projects: `[Domain].[Layer]` or `Bureau.[Concern]`
- Controllers: `{Entity}Controller` inheriting `BureauApiControllerBase`
- Services: `{Entity}Service` implementing `I{Entity}Service`
- DTOs: `{Entity}Dto`, `Create{Entity}Request`, `Update{Entity}Request`
- Mappers: extension methods in `{Entity}MapperExtension.cs`
- EF configs: `{Entity}TypeConfiguration.cs`
- DI registration: extension methods in `ServiceCollectionExtensions.cs` / `IServiceCollectionExtension.cs`

### Frontend
- Apps: kebab-case (`bureau-web`, `niles-chores-web`)
- Libs: kebab-case (`bureau-shell`, `client-core`)
- Components: PascalCase (`Header.tsx`, `Sidebar.tsx`)
- Feature directories: `features/` with feature key constants in `featureKeys.ts`

## Port Assignments (5NXX scheme)

| Port | Service |
|---|---|
| 5000 | bureau-web SPA |
| 5200 | Watson.Nodes.Api |
| 5210 | Watson.Items.Ingest.Api |
| 5300 | Niles.Chores.Api |
