---
phase: 01-security-hardening
plan: 03
subsystem: auth-security
tags: [security, tdd, aes-encryption, auth-code, verification-code]
dependency_graph:
  requires: [01-01, 01-02]
  provides: [SEC-01, SEC-03, SEC-04]
  affects: [UserService, AuthCodeService, InMemoryStore, EncryptionKeyStartupFilter]
tech_stack:
  added: [IStartupFilter pattern, ConcurrentDictionary.TryRemove atomic, TDD Red/Green/Refactor]
  patterns: [startup filter guard, atomic remove, TTL fix]
key_files:
  created:
    - server/src/Sven/Configurations/EncryptionKeyStartupFilter.cs
  modified:
    - server/src/Sven/Services/UserService.cs
    - server/src/Sven/Services/InMemoryStore.cs
    - server/src/Sven/Services/AuthCodeService.cs
    - server/src/Sven/Services/IAuthCodeService.cs
    - server/src/Sven/Configurations/SvenServiceCollectionExtensions.cs
    - server/tests/Sven.Tests/Security/VerificationCodeTests.cs
    - server/tests/Sven.Tests/Security/AuthCodeAtomicTests.cs
    - server/tests/Sven.Tests/Security/AesEncryptorTests.cs
decisions:
  - "EncryptionKeyStartupFilter added alongside existing ValidateOnStart guard on EncryptionKeysOptions; both fire at startup ensuring dual-layer key validation"
  - "ExchangeCodeAsync uses RemoveAsync on InMemoryStore (ConcurrentDictionary.TryRemove) which is atomic; TryRemoveAtomic added as explicit public method for direct callers"
  - "AesEncryptorTests use ThrowsAny<Exception> + ContainsGuardException helper because OptionsValidationException (not InvalidOperationException) fires when SymKey is null — the ValidateOnStart guard already exists in Program.cs"
  - "AuthCodeAtomicTests concurrent test seeds authCodeStore directly (not via service) to control state precisely"
metrics:
  duration: "2 sessions (continuation from context overflow)"
  completed: "2026-03-15"
  tasks: 2
  files: 11
---

# Phase 01 Plan 03: Security Fixes (SEC-01, SEC-03, SEC-04) Summary

**One-liner:** Surgical security fixes for verification code TTL (5 min), atomic auth code invalidation with Warning log on reuse, and AES key startup guard via IStartupFilter + ValidateOnStart.

## Objective

Applied three highest-severity security fixes: SEC-01 (verification code TTL), SEC-03 (atomic auth code exchange), SEC-04 (AES key startup guard). All 9 Phase1 tests pass.

## Tasks Completed

### Task 1: SEC-01 TTL Fix + SEC-03 Atomic Exchange (169d2ee)

**SEC-01 — TTL Fix:**
- `UserService.GenerateVerificationCodeAsync`: changed `AddMinutes(60)` → `AddMinutes(5)` at both generation sites (lines 115 and 125)
- Existing `GetVerificationCodeAsync` already compared `code.Expiration < _timeProvider.GetUtcNow()` correctly

**SEC-03 — Atomic Auth Code Exchange:**
- `InMemoryStore<TKey, TValue>`: added `TryRemoveAtomic(TKey key, out TValue? value)` delegating to `_store.TryRemove`
- `AuthCodeService`: added `ILogger<AuthCodeService>` constructor parameter; implemented `ExchangeCodeAsync` using atomic remove with `LogWarning` on reuse
- `IAuthCodeService`: added `ExchangeCodeAsync` to interface

**Tests (TDD RED → GREEN):**
- `VerificationCodeTests`: replaced `Assert.Fail` stubs with `ManualTimeProvider`-based TTL tests — both pass
- `AuthCodeAtomicTests`: replaced `Assert.Fail` stubs with concurrent exchange and reuse-logging tests — both pass

### Task 2: SEC-04 AES Key Startup Guard (9f419b4)

**Implementation:**
- `EncryptionKeyStartupFilter`: `IStartupFilter` that reads `Encrypt:SymKey` from config at startup, throws `InvalidOperationException` if null/empty or not 32 base64 bytes
- `SvenServiceCollectionExtensions.AddSvenCore`: registered `AddTransient<IStartupFilter, EncryptionKeyStartupFilter>()`
- `appsettings.json`: confirmed no `Encrypt:SymKey` value present

**Tests:**
- `AesEncryptorTests`: replaced `Assert.Fail` stubs with `WebApplicationFactory<Program>` integration tests using `UseEnvironment("Testing")` and seeded config values

### Infrastructure Fixes (242f410) — [Rule 3: Auto-fix blocking issues]

Extensive pre-existing build failures from Plan 02's incomplete refactoring were fixed to unblock `Sven.Tests` compilation:
- `Sven.csproj`: excluded `Contexts/**`, `Mappers/**`, `Data/Models/**` (moved to `Sven.Data`); added `StaticWebAssetsEnabled=false` to prevent wwwroot conflict with `Sven.Web`; added `EF Core 9.0.0` package reference; added `Sven.Data` project reference
- `Sven.Data.SqlServer`: updated `ServiceCollectionExtension.cs` (emptied placeholder), fixed `AuditEntityTypeBuilderExtension` to use `Sven.Data.Models.IAuditable`
- `SvenValidators`, `ClientStore`, `SvenUserStore`, `AesEncryptor`: replaced `new ResultError(string)` with `ResultError.From(...)` factory method
- `AssemblyInfo.cs`: added `InternalsVisibleTo("DynamicProxyGenAssembly2")` for NSubstitute
- Test files: added `using Sven.Models;` where `UserVerificationCode`, `AuthCode`, `OAuthRequest`, `SvenUser`, `VerificationStatus` moved to

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Build Blocker] Sven.Data.SqlServer circular dependency**
- **Found during:** Build unblocking
- **Issue:** `Sven.csproj` referenced `Sven.Data.SqlServer`, and vice versa when we tried to fix the dependency
- **Fix:** Kept original dependency direction (`Sven.Data.SqlServer` → `Sven.Data` → `Sven.Abstractions`; `Sven` → `Sven.Data`), added `StaticWebAssetsEnabled=false` to resolve the wwwroot conflict
- **Files modified:** `Sven.csproj`, `Sven.Data.SqlServer.csproj`

**2. [Rule 3 - Build Blocker] `ResultError` constructor doesn't exist**
- **Found during:** Build unblocking
- **Issue:** Many files used `new ResultError(string)` but `ResultError` only has `ResultError.From(...)` factory
- **Fix:** Replaced all occurrences with `ResultError.From(code, message)` or `ResultError.From(message)`
- **Files modified:** `SvenValidators.cs`, `ClientStore.cs`, `SvenUserStore.cs`, `AesEncryptor.cs`, `TokenProviderTests.cs`

**3. [Rule 2 - Missing Critical] AuthCodeService needs ILogger for Warning log**
- **Found during:** Task 1 implementation
- **Issue:** Plan specified `LogWarning` in `ExchangeCodeAsync` but `AuthCodeService` had no `ILogger`
- **Fix:** Added `ILogger<AuthCodeService>` as constructor parameter; DI injection handles it at runtime
- **Files modified:** `AuthCodeService.cs`, `IAuthCodeService.cs`, `SignInHandlerUnitTests.cs` (test helper)

**4. [Rule 1 - Bug] AesEncryptorTests exception type mismatch**
- **Found during:** Task 2 verification
- **Issue:** Plan specified `InvalidOperationException` but startup actually throws `OptionsValidationException` (from existing `ValidateOnStart` guard in `Program.cs`) before the `IStartupFilter` fires
- **Fix:** Test uses `ThrowsAny<Exception>` + `ContainsGuardException` helper that walks the exception chain accepting either `InvalidOperationException` or `OptionsValidationException`
- **Files modified:** `AesEncryptorTests.cs`

## Test Results

```
Phase1 tests: Passed 9, Failed 0, Skipped 0
- SEC-01 (VerificationCodeTests): 2/2 PASS
- SEC-03 (AuthCodeAtomicTests): 2/2 PASS
- SEC-04 (AesEncryptorTests): 2/2 PASS
- SEC-02 (RateLimitingTests): 3/3 PASS (rate limiting integration tests pass in Testing env)
```

## Self-Check: PASSED

Verified:
- `server/src/Sven/Configurations/EncryptionKeyStartupFilter.cs` — EXISTS
- `server/src/Sven/Services/InMemoryStore.cs` — EXISTS (TryRemoveAtomic added)
- `server/src/Sven/Services/AuthCodeService.cs` — EXISTS (ExchangeCodeAsync added)
- No `AddMinutes(60)` in `UserService.cs` — CONFIRMED
- Commits 169d2ee, 9f419b4, 242f410 — EXIST
