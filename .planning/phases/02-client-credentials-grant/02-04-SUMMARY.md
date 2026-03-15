---
phase: 02-client-credentials-grant
plan: "04"
subsystem: auth
tags: [oauth2, client-credentials, jwt, rfc9068, rfc6749, aspnetcore]

requires:
  - phase: 02-client-credentials-grant/02-02
    provides: HashedSecret column and ClientRepository wired for confidential clients
  - phase: 02-client-credentials-grant/02-03
    provides: IClientAuthService with Basic and Post authentication extraction and verification

provides:
  - client_credentials grant dispatched in TokenController.TokenAsync
  - HandleClientCredentialsFlow: client auth → scope validation → machine token issuance
  - CreateMachineTokenAsync on ITokenProvider interface and SvenTokenProvider implementation
  - RFC 9068 machine JWT: iss, aud=issuer, exp, iat, jti, scope, client_id — no sub, no refresh_token, no id_token
  - IClientAuthService and ITokenProvider factory registered in AddSvenCore()
  - 9 passing integration tests covering full RFC 6749 §4.4 contract

affects:
  - 02-05-plan (token introspection may reference machine token claim shape)
  - 02-client-credentials-grant (grant is now end-to-end operational)

tech-stack:
  added: []
  patterns:
    - IClientAuthService resolved via HttpContext.RequestServices (not constructor) to avoid public-ctor accessibility violation for internal types
    - ITokenProvider registered via factory lambda in AddSvenCore() to bypass DI reflection failure on internal constructor
    - EF in-memory test context: Ignore<SerializedData> + direct Configure() call for open-generic type configurations
    - RFC 9068 machine token: aud=issuer (not _jwtOptions.Audience), no sub claim

key-files:
  created:
    - server/tests/Sven.Tests/ClientCredentials/Rfc6749ClientCredentialsGrantTests.cs
  modified:
    - server/src/Sven.Abstractions/Services/ITokenProvider.cs
    - server/src/Sven/Services/SvenTokenProvider.cs
    - server/src/Sven.Web/Controllers/Connect/TokenController.cs
    - server/src/Sven/Configurations/SvenServiceCollectionExtensions.cs
    - server/src/Sven.Web/Program.cs
    - server/src/Sven.Contracts/TokenRequest.cs
    - server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs

key-decisions:
  - "IClientAuthService resolved via HttpContext.RequestServices in TokenController — internal type cannot be a public constructor parameter; runtime resolution avoids CS0051 accessibility violation"
  - "ITokenProvider registered via factory lambda inside AddSvenCore() — SvenTokenProvider constructor is internal and uses internal IStore<> parameter; reflection-based DI would fail at runtime"
  - "Machine token aud claim = _jwtOptions.Issuer (not Audience) — RFC 9068 §2.2: for client_credentials the aud identifies the authorization server, not a resource server"
  - "EF in-memory does not support ApplyConfigurationsFromAssembly for open-generic IEntityTypeConfiguration<T>; test context must call Configure() directly on the concrete type"

patterns-established:
  - "Machine token pattern: no sub, no refresh_token, no id_token; scope + client_id carry all authorization context"
  - "Scope validation: omitted request scope → all registered scopes; requested subset → intersection; requested superset → 400 invalid_scope"

requirements-completed:
  - PROT-01

duration: 40min
completed: "2026-03-15"
---

# Phase 02 Plan 04: Client Credentials Grant End-to-End Summary

**POST /connect/token with grant_type=client_credentials now issues RFC 9068 machine JWTs (no sub, no refresh token) after Basic or Post client authentication and scope validation, with 9 integration tests covering the full RFC 6749 §4.4 contract.**

## Performance

- **Duration:** ~40 min
- **Started:** 2026-03-15T18:30:00Z
- **Completed:** 2026-03-15T19:10:00Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments

- End-to-end client_credentials grant: client auth (plan 03) wired to machine token issuance through TokenController
- RFC 9068-compliant machine token: iss, aud=issuer, exp, iat, jti, scope, client_id; no sub, no refresh_token, no id_token
- Nine integration tests all passing, covering both auth methods (Basic/Post), scope subset/superset/omit, unknown client, wrong secret, dual-method rejection

## Task Commits

Each task was committed atomically:

1. **Task 1: Add CreateMachineTokenAsync to ITokenProvider and SvenTokenProvider** - `97d3a86` (feat)
2. **Task 2: Add HandleClientCredentialsFlow to TokenController and register IClientAuthService** - `62b7c6f` (feat)

**Plan metadata:** (docs commit — pending)

## Files Created/Modified

- `server/src/Sven.Abstractions/Services/ITokenProvider.cs` — Added `CreateMachineTokenAsync` signature
- `server/src/Sven/Services/SvenTokenProvider.cs` — Implemented `CreateMachineTokenAsync` with RFC 9068 claim set
- `server/src/Sven.Web/Controllers/Connect/TokenController.cs` — Added `client_credentials` branch and `HandleClientCredentialsFlow`
- `server/src/Sven/Configurations/SvenServiceCollectionExtensions.cs` — Registered `IClientAuthService` and `ITokenProvider` factory
- `server/src/Sven.Web/Program.cs` — Removed duplicate `ITokenProvider` direct registration (was overriding factory)
- `server/src/Sven.Contracts/TokenRequest.cs` — Fixed `RedirectUri` to `string?`, added `ClientSecret` property
- `server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs` — Added `OnModelCreating` override: `Ignore<SerializedData>` + direct `ClientBaseTypeConfiguration<ClientDb>.Configure()`
- `server/tests/Sven.Tests/ClientCredentials/Rfc6749ClientCredentialsGrantTests.cs` — Implemented all 9 integration tests

## Decisions Made

- **IClientAuthService via RequestServices:** Internal type cannot appear in public constructor signature (CS0051). Runtime resolution via `HttpContext.RequestServices.GetRequiredService<IClientAuthService>()` inside the method body avoids the violation while keeping the service scoped correctly.

- **ITokenProvider factory in AddSvenCore():** `SvenTokenProvider` has an `internal` constructor and depends on `IStore<string, RefreshToken>` (also internal). The DI container uses reflection to find `public` constructors and would throw at runtime. A factory lambda registered inside the `Sven` project (where all types are visible) solves this without making internals public.

- **Machine token aud = issuer:** RFC 9068 §2.2 specifies that for pure client credentials the `aud` identifies the authorization server itself, not a downstream resource. `_jwtOptions.Issuer` is used, not `_jwtOptions.Audience`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] SvenTokenProvider internal constructor caused 500 on all token endpoints**
- **Found during:** Task 2 (running integration tests)
- **Issue:** `SvenTokenProvider` constructor is `internal` and accepts `IStore<string, RefreshToken>` (also `internal`). The ASP.NET Core DI container uses reflection to discover `public` constructors, throwing `InvalidOperationException` at runtime for every token request.
- **Fix:** Added factory lambda registration in `AddSvenCore()` (inside Sven project where internal types are accessible). Removed `builder.Services.AddScoped<ITokenProvider, SvenTokenProvider>()` from `Program.cs` which was overriding the factory.
- **Files modified:** `SvenServiceCollectionExtensions.cs`, `Program.cs`
- **Verification:** Token endpoint returns 200 (was 500); all test baseline maintained.
- **Committed in:** `62b7c6f` (Task 2 commit)

**2. [Rule 1 - Bug] TokenRequest.cs in Sven.Contracts had [Required] on RedirectUri blocking client_credentials**
- **Found during:** Task 2 (integration tests returning 400 on valid client_credentials requests)
- **Issue:** Plan 02-03 had fixed `Sven/Models/TokenRequest.cs` (namespace `Sven.Models`) but the controller actually binds from `Sven.Contracts/TokenRequest.cs` (namespace `Sven`). The Contracts file still had `[Required]` on `RedirectUri` and was missing the `ClientSecret` property entirely.
- **Fix:** Fixed `Sven.Contracts/TokenRequest.cs`: removed `[Required]` from `RedirectUri`, made it `string?`, added `ClientSecret` property.
- **Files modified:** `server/src/Sven.Contracts/TokenRequest.cs`
- **Verification:** client_credentials requests no longer rejected for missing redirect_uri.
- **Committed in:** `62b7c6f` (Task 2 commit)

**3. [Rule 1 - Bug] EF Core in-memory provider discovered SerializedData as keyless entity causing PrimaryKeyRequiredException**
- **Found during:** Task 2 (SeedConfidentialClientAsync throwing on first test run)
- **Issue:** EF Core in-memory provider's `ApplyConfigurationsFromAssembly` cannot instantiate open-generic `IEntityTypeConfiguration<T>` implementations. Without the type configuration applied, EF treats `SerializedData` (a reference-type property of `ClientDb`) as a navigation property requiring a primary key.
- **Fix:** Added `OnModelCreating` override to `SvenTestContext`: `modelBuilder.Ignore<SerializedData>()` prevents EF from treating it as an entity; `new ClientBaseTypeConfiguration<ClientDb>().Configure(modelBuilder.Entity<ClientDb>())` applies the `HasConversion` JSON serialization for `ClientAddendum`.
- **Files modified:** `server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs`
- **Verification:** Client seeds successfully; `ClientAddendum` round-trips correctly via JSON HasConversion.
- **Committed in:** `62b7c6f` (Task 2 commit)

**4. [Rule 2 - Missing] Implemented all 9 Rfc6749ClientCredentialsGrantTests integration tests**
- **Found during:** Task 2 (tests existed as `Assert.Fail("not implemented")` stubs per plan 02-01)
- **Issue:** Tests needed to be implemented to verify the grant flow. Plan specified the behaviors; tests were planned stubs awaiting implementation.
- **Fix:** Implemented all 9 tests using `IClientRepository` seeding, `JwtSecurityTokenHandler` claim inspection, and both auth methods.
- **Files modified:** `server/tests/Sven.Tests/ClientCredentials/Rfc6749ClientCredentialsGrantTests.cs`
- **Verification:** All 9 tests pass; no regressions in the 67 previously-passing tests.
- **Committed in:** `62b7c6f` (Task 2 commit)

---

**Total deviations:** 4 auto-fixed (2 Rule 1 - Bug, 1 Rule 1 - Bug infrastructure, 1 Rule 2 - Missing)
**Impact on plan:** All fixes were blocking — without them the grant flow could not be exercised at all. No scope creep; all fixes directly enabled completing the planned tasks.

## Issues Encountered

- The `ResultError` struct properties in this codebase are `.Code` and `.ErrorMessage`, not `.Error` and `.Description` as the plan's code snippet assumed. Corrected during implementation.
- `ApplyConfigurationsFromAssembly` fails silently for open-generic configurations in EF Core 9 in-memory provider — requires direct `Configure()` invocation as a workaround.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `POST /connect/token` with `grant_type=client_credentials` is fully operational end-to-end
- Machine token claim shape is established (iss, aud=issuer, exp, iat, jti, scope, client_id)
- Plan 02-05 (token introspection) can proceed immediately; introspection endpoint will inspect machine tokens using the same claim set
- No blockers

---
*Phase: 02-client-credentials-grant*
*Completed: 2026-03-15*
