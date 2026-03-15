# Technology Stack

**Analysis Date:** 2026-03-15

## Languages

**Primary:**
- C# (net8.0) — Backend APIs, database contexts, business logic
- TypeScript (5.6+) — Frontend SPAs, shared libraries, mobile app
- JavaScript — Package management, build tooling

**Secondary:**
- SCSS — Styling (bureau-web, archived Sven.Web pages)
- HTML/Razor — Sven.Web pages (auth UI, deprecated after restructuring)

## Runtime

**Environment:**
- .NET 8.0 (dotnet SDK required for server)
- Node.js LTS (required for frontend workspace)
- docker — Multi-stage builds for production

**Package Manager:**
- pnpm 10.26.2 — Frontend workspace manager
- dotnet CLI — C# package management via NuGet

**Lockfile:**
- `app/pnpm-lock.yaml` — Frontend lockfile (committed)
- NuGet implicit in `.csproj` via package version pins

## Frameworks

**Core (Backend):**
- ASP.NET Core 8.0 — Web API framework for all backends
- Entity Framework Core 8.0 / 9.0 — ORM for database abstraction
- Microsoft.IdentityModel.Tokens (8.7.0) — JWT validation
- System.IdentityModel.Tokens.Jwt (8.7.0) — JWT signing

**Database (Abstraction):**
- Entity Framework Core (8.0.22) — Base ORM
- Microsoft.EntityFrameworkCore.SqlServer (8.0.22, 9.0.0) — SQL Server provider
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.4) — PostgreSQL provider
- Microsoft.EntityFrameworkCore.InMemory (8.0.0) — Test in-memory database

**Frontend:**
- React 18.3.1 — UI library (web and mobile)
- React Router DOM 7.0.0 — Routing (web only)
- React Native 0.81.5 — Mobile framework (via Expo)
- Expo 54.0.33 — React Native runtime and tooling

**Testing:**
- xUnit 2.5.3 — C# test framework
- NSubstitute 5.3.0 — Mocking for C# tests
- Microsoft.AspNetCore.Mvc.Testing (8.0.15) — Integration test helpers
- Microsoft.EntityFrameworkCore.InMemory (8.0.0) — In-memory test database
- coverlet.collector (6.0.0) — Code coverage

**Build/Dev (Frontend):**
- Vite 6.0+ — Build tool and dev server
- @vitejs/plugin-react (4.3.4) — React Fast Refresh
- TypeScript (5.6+) — Type checking

**Build/Dev (Backend):**
- Microsoft.NET.Test.Sdk (17.8.0) — Test running infrastructure
- Microsoft.EntityFrameworkCore.Design (8.0.22, 9.0.0) — Migrations and scaffolding

**Frontend-Only Packages:**
- Swashbuckle.AspNetCore (6.6.2, 8.1.4) — Swagger/OpenAPI generation
- JavaScriptEngineSwitcher.V8 (3.29.1) — V8 JavaScript engine (for Sven.Web, deprecated)
- LigerShark.WebOptimizer.Sass (3.0.123) — SCSS compilation (for Sven.Web, deprecated)
- @types/react (18.3.27) — React type definitions

## Key Dependencies

**Critical:**
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.22) — JWT middleware
- Microsoft.AspNetCore.Authentication.OpenIdConnect (8.0.22) — OIDC client
- Microsoft.AspNetCore.Authentication.Google (8.0.14) — External provider (Sven.Web)
- Microsoft.AspNetCore.Authentication.MicrosoftAccount (8.0.14) — External provider (Sven.Web)
- Microsoft.AspNetCore.Authentication.Cookies (2.3.0) — Cookie auth (Sven.Web)

**Infrastructure:**
- Microsoft.Extensions.Options.ConfigurationExtensions (8.0.0) — Configuration binding
- Microsoft.Extensions.Diagnostics.HealthChecks — Health check endpoints
- react-dom (18.3.1) — React DOM bindings

**Mobile-Only:**
- expo-sqlite (15.2.0) — Local SQLite database
- @react-native-community/netinfo (12.0.0) — Network detection
- @react-navigation/* (7.0.0+) — Navigation stack (native-stack, bottom-tabs)
- react-native-safe-area-context (5.6.2) — Safe area handling
- react-native-screens (4.16.0) — Native screen management

## Configuration

**Backend:**
- Configuration via `appsettings.json` + `appsettings.{Environment}.json`
- Support for environment variable overrides (ASP.NET Core standard `__` separator)
- Sections: Auth (mode, dev claims), ConnectionStrings (per-database), Logging, Jwt, TokenVault, Cors

**Frontend:**
- Runtime config via `config.json` (gitignored, populated from `config.template.json`)
- Environment variables: `BUREAU_ENVIRONMENT`, `BUREAU_AUTH_MODE`, `BUREAU_OIDC_*`, `BUREAU_API_*`
- Docker entrypoint substitutes env vars into config.template.json at container startup

**Build:**
- `app/pnpm-workspace.yaml` — Workspace root definition
- `app/package.json` — Root scripts for `pnpm build` and `pnpm dev`
- `server/Bureau.slnx` — .NET solution manifest
- `app/client/apps/bureau-web/vite.config.ts` — Vite dev server port 5000, path aliases
- `tsconfig.json` per package — TypeScript strict mode enabled

## Platform Requirements

**Development:**
- .NET 8 SDK
- Node.js LTS + pnpm@10.26.2
- Docker (SQL Server container)

**Production:**
- ASP.NET Core 8.0 runtime (Linux container)
- Node.js 20+ (frontend build environment only, not runtime)
- Nginx (static SPA serving)
- SQL Server or PostgreSQL (persistent storage)

---

*Stack analysis: 2026-03-15*
