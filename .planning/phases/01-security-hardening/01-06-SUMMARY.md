---
phase: 01-security-hardening
plan: "06"
subsystem: Sven / Sven.Abstractions
tags:
  - layer-boundary
  - naming-conventions
  - CODE-01
  - CODE-02
  - dependency-cleanup
dependency_graph:
  requires:
    - "01-02 (IUserService/IClientService/IUserRepository established)"
  provides:
    - "Clean Sven.Abstractions — only public interfaces Sven.Web injects directly"
    - "IStore/IExternalTokenRefresher as internal Sven types registered centrally"
    - "SvenServiceCollectionExtensions as single DI registration point for all Sven-internal services"
  affects:
    - "Sven.Data.Postgres/SqlServer (IUserStore/IClientStore implementations removed)"
    - "All code that previously used IClientProvider/IUserProvider/IClientStore/IUserStore"
tech_stack:
  added: []
  patterns:
    - "Internal interfaces registered via public extension method (InternalsVisibleTo not required for DI)"
    - "IUserService.GetUserByUsernameAsync added to bridge UserClaimsProvider -> IUserStore removal"
key_files:
  created:
    - server/src/Sven/Services/IStore.cs
    - server/src/Sven/Services/IExternalProviderRegistry.cs
    - server/src/Sven/Services/IExternalTokenRefresher.cs
  modified:
    - server/src/Sven/Configurations/SvenServiceCollectionExtensions.cs
    - server/src/Sven/Services/SvenTokenProvider.cs
    - server/src/Sven/Services/UserClaimsProvider.cs
    - server/src/Sven/Services/UserService.cs
    - server/src/Sven/Data/Repositories/UserRepository.cs
    - server/src/Sven.Abstractions/Services/IUserService.cs
    - server/src/Sven.Data/Stores/SvenUserStore.cs
    - server/src/Sven.Data/Stores/ClientStore.cs
    - server/src/Sven.Data/Configurations/ServiceCollectionExtension.cs
    - server/src/Sven.Web/Program.cs
    - server/tests/Sven.Tests/Services/TokenProviderTests.cs
    - server/tests/Sven.Tests/Stores/SvenUserStoreTests.cs
    - server/tests/Sven.Tests/Stores/ClientStoreTests.cs
    - server/tests/Sven.Tests/Security/AuthCodeAtomicTests.cs
    - server/tests/Sven.Tests/Pages/Connect/SignIn/SignInHandlerUnitTests.cs
    - server/tests/Sven.Tests/Pages/Connect/SignUp/PlainSignUpTests.cs
  deleted:
    - server/src/Sven.Abstractions/Data/IClientStore.cs
    - server/src/Sven.Abstractions/Data/IUserStore.cs
    - server/src/Sven.Abstractions/Services/IClientProvider.cs
    - server/src/Sven.Abstractions/Services/IUserProvider.cs
    - server/src/Sven/Services/ClientProvider.cs
    - server/src/Sven/Services/UserProvider.cs
decisions:
  - "IExternalProviderRegistry kept public (not internal) in Sven — Sven.Web page models inject it by constructor parameter name; making it internal would cause CS0246 compile errors across eight Sven.Web files"
  - "IStore<string,RefreshToken> registration added to AddSvenCore — was missing from Program.cs; SvenTokenProvider injects it and runtime DI would fail without it"
  - "GetUserByUsernameAsync added to IUserService — UserClaimsProvider.GetClaimsPrincipalAsync(username,password) needed username lookup after IUserStore was removed; routing through IUserService maintains layer boundary"
  - "SvenTokenProvider constructor changed from public to internal — public constructor with internal parameter type violates CS0051 accessibility rules"
metrics:
  duration_minutes: 25
  completed_date: "2026-03-15"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 16
  files_deleted: 6
  files_created: 3
---

# Phase 1 Plan 06: Layer Boundary Enforcement (CODE-01/CODE-02) Summary

**One-liner:** Relocated IStore/IExternalTokenRefresher to Sven as internal types; deleted all *Store/*Provider dead code; centralized DI registration in SvenServiceCollectionExtensions; all consumers updated to *Service/*Repository naming.

## What Was Built

Completed CODE-01 layer boundary enforcement and CODE-02 naming cleanup:

- **IStore, IExternalTokenRefresher** moved from `Sven.Abstractions/Services/` to `Sven/Services/` as `internal` interfaces. DI registration moved into `SvenServiceCollectionExtensions.AddSvenCore()` so `Sven.Web` resolves them without naming internal types.
- **IExternalProviderRegistry** moved from `Sven.Abstractions/Services/` to `Sven/Services/` as `public` (see deviation below for why it cannot be internal).
- **IClientStore, IUserStore, IClientProvider, IUserProvider** deleted entirely from `Sven.Abstractions`. Implementations and dead code removed from `Sven` and `Sven.Data`.
- **SvenTokenProvider** updated to inject `IClientService` (not `IClientProvider`).
- **UserClaimsProvider** updated to inject `IUserService` (not `IUserStore`).
- **SvenServiceCollectionExtensions** is now the single DI registration point for all Sven-internal services: all `IStore<>` variants, `IExternalTokenRefresher`, `IExternalProviderRegistry`.
- **Program.cs** (`Sven.Web`) no longer references any internal Sven types by name.
- Tests updated to use `IClientService`, `IUserService`, `IUserRepository`, `IClientRepository` and concrete `InMemoryStore<>` where `AuthCodeService` requires it.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] IExternalProviderRegistry kept public in Sven/Services/**
- **Found during:** Task 1 verification
- **Issue:** The plan stated `IExternalProviderRegistry` can be `internal` to Sven, but eight `Sven.Web` files inject it directly by name in constructor parameters (`ExternalAuthServiceCollectionExtensions`, six page models, one page). Making it `internal` causes CS0246 compile errors across all those files.
- **Fix:** Interface moved from `Sven.Abstractions/Services/` to `Sven/Services/` but kept `public`. The layer boundary still improves — it's no longer in Abstractions — but it is accessible from `Sven.Web` which references `Sven`.
- **Files modified:** `server/src/Sven/Services/IExternalProviderRegistry.cs`

**2. [Rule 2 - Missing] IStore<string,RefreshToken> registration added to AddSvenCore**
- **Found during:** Task 2 review
- **Issue:** `SvenTokenProvider` injects `IStore<string, RefreshToken>` but this registration was absent from `Program.cs`. This would cause a runtime DI resolution failure.
- **Fix:** Added `services.AddSingleton<IStore<string, RefreshToken>, InMemoryStore<string, RefreshToken>>()` to `AddSvenCore()`.
- **Files modified:** `server/src/Sven/Configurations/SvenServiceCollectionExtensions.cs`

**3. [Rule 1 - Bug] GetUserByUsernameAsync added to IUserService**
- **Found during:** Task 2 (UserClaimsProvider fix)
- **Issue:** After `IUserStore` was deleted, `UserClaimsProvider.GetClaimsPrincipalAsync(username, password)` had no way to look up a user by username — `IUserService` only exposed `GetUserByIdAsync`. `IUserRepository` is `internal` and inaccessible from `Sven.Services`.
- **Fix:** Added `GetUserByUsernameAsync` to `IUserService` interface and implemented it in `UserService` via `_userRepository.GetByUsernameAsync`. `UserClaimsProvider` now routes through `IUserService`.
- **Files modified:** `server/src/Sven.Abstractions/Services/IUserService.cs`, `server/src/Sven/Services/UserService.cs`, `server/src/Sven/Services/UserClaimsProvider.cs`

**4. [Rule 1 - Bug] SvenTokenProvider constructor changed from public to internal**
- **Found during:** Task 2 build (CS0051 error)
- **Issue:** `SvenTokenProvider` is `public` but its constructor took `IStore<string, RefreshToken>` (now `internal`). C# rule CS0051: parameter type accessibility must be at least as accessible as the containing method.
- **Fix:** Constructor changed from `public` to `internal`. DI registration (`AddScoped<ITokenProvider, SvenTokenProvider>()`) is inside `Sven` project and can access the internal constructor.
- **Files modified:** `server/src/Sven/Services/SvenTokenProvider.cs`

**5. [Rule 1 - Bug] Test files updated to remove deleted interface references**
- **Found during:** Task 2 (test build)
- **Issue:** Six test files referenced `IClientProvider`, `IClientStore`, `IUserStore`, `IUserProvider`, and used `IStore<>` where `AuthCodeService` requires the concrete `InMemoryStore<>`.
- **Fix:** Updated tests to use `IClientService`, `IUserService`, `IUserRepository`, `IClientRepository`, and `InMemoryStore<>` directly.
- **Files modified:** Six test files in `server/tests/Sven.Tests/`

## Build Results

All Sven-related projects build with zero errors:
- `Sven.csproj` — Build succeeded, 0 errors
- `Sven.Web.csproj` — Build succeeded, 0 errors
- `Sven.Tests.csproj` — Build succeeded, 0 errors

Pre-existing failures in `Watson.Nodes.Api` and `Niles.Chores.Api` (missing `Bureau.AspNetCore` reference) are out of scope for this plan.

## Self-Check: PASSED

All key files verified on disk. Both task commits exist in git history (5124c0d, 2ac120f). Deleted files confirmed absent. Build verified: 0 errors across Sven, Sven.Web, Sven.Tests.
