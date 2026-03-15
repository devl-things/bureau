# Concerns

## Tech Debt

### 1. In-memory job state — no persistence across restarts
`Watson.Items.Ingest` uses `JobManagerInMemory` (`server/src/Watson.Items.Ingest/Jobs/JobManagerInMemory.cs`) backed by `ConcurrentDictionary` and an unbounded `Channel<T>`. All job state (status, progress, cancellation tokens) is lost on restart. For a background ETL service this is a significant durability gap — any in-flight job is silently dropped.

### 2. Seeding architecture — dev-only seed path in production code
`Niles.Chores.Api/Program.cs` calls `app.Services.SeedChores()` inside `if (app.Environment.IsDevelopment())`. Seeding is wired in Program.cs rather than via a separate CLI/migration tool. A `TODO #66` comment in that file also indicates `SEEDER_USAGE.md` needs updating, suggesting this area was recently changed without follow-through.

### 3. FeatureKeys sync across frontend/backend
`FeatureKeys.cs` (`Bureau.Primitives/Features/FeatureKeys.cs`) and `featureKeys.ts` (`app/client/apps/bureau-web/src/features/featureKeys.ts`) must stay in sync manually. There is no code-gen or shared contract — drift will silently break feature gating without compile-time errors.

### 4. Sven migration in progress
The `feature/sven` branch shows Sven undergoing significant restructuring (many files deleted from `server/src/Sven/`, new `Sven.Web/` project added). The old `server/src/Sven/` is being replaced by `server/src/Sven.Web/` but both coexist in the solution during transition. Test files still reference deleted paths.

### 5. Watson.Nodes.Api still uses `var`
`server/src/Watson.Nodes.Api/Program.cs:19` uses `var builder = WebApplication.CreateBuilder(args)` — violating the project's no-`var` convention defined in `CLAUDE.md`. Not applied consistently across the Watson.Nodes service.

### 6. `IProcessedStore` — file-based processed items tracking
`Watson.Items.Ingest` uses `FileProcessedStore` to track already-processed ETL files. File-based state is fragile in containerized environments where the filesystem may not persist.

### 7. Watson.Items — namespace mismatch
`JobManagerInMemory.cs` declares `namespace Niles.Etl.Jobs` but lives in `server/src/Watson.Items.Ingest/Jobs/`. This is a stale namespace from a rename — it will compile but is misleading.

## Known Issues / Bugs

### 1. External auth flow (Sven) — incomplete token refresh handling
`Sven/Services/ExternalTokenRefresher.cs` is a new untracked file (per git status) indicating the external token refresh feature is mid-development. The background service `TokenRefreshBackgroundService.cs` is also untracked — these features are not yet integrated or tested.

### 2. Test infrastructure references deleted files
`server/tests/Sven.Tests/` has modified test files (`ConnectControllerTests.cs`, `SvenWebAppFactory.cs`, `SignUpTests`, etc.) and these reference types from deleted projects. Tests likely won't compile until Sven restructuring completes.

## Security

### 1. Encryption key management (Sven)
`Sven/Configurations/EncryptionKeysOptions.cs` and `TokenVaultOptions.cs` suggest symmetric encryption keys are config-bound. If these keys leak through config (appsettings, env vars in logs), tokens are compromised. No KMS integration visible.

### 2. Dev auth handler bypass
`DevApiTokenAuthenticationHandler` and `DevPrincipalFactory` inject arbitrary claims including all feature scopes when `Features` list is empty. If this handler is accidentally enabled in non-dev environments (misconfigured `ASPNETCORE_ENVIRONMENT`), it grants full access. The guard is a simple environment check.

### 3. PKCE / OIDC security surface (Sven)
Sven implements PKCE, authorization codes, refresh tokens, and external provider token storage. This is a large security surface maintained as first-party code. Any implementation bugs (auth code reuse, token leakage, PKCE bypass) have significant impact.

### 4. External tokens stored encrypted — key rotation
`Sven` stores external provider tokens encrypted (`AesEncryptor`). AES key rotation is not implemented — rotating keys would require re-encrypting all stored tokens.

## Performance

### 1. No pagination on chores search
`ChoresController` uses `SearchParameters` but the base query patterns don't show cursor pagination being enforced. `Bureau.Primitives` has `CursorResult<T>` and `CursorParameters` — Watson.Nodes uses these correctly, but Niles.Chores may not.

### 2. Watson.Nodes node attribute storage
`NodeAttribute` stores typed values with an `AttributeValueType` enum. Queries filtering on attribute values likely require full table scans depending on index coverage.

### 3. Items ingest unbounded channel
`JobManagerInMemory` uses `Channel.CreateUnbounded<JobWorkItem>()` — if jobs are enqueued faster than processed (e.g., batch imports), memory grows unbounded.

### 4. Sven refresh token background service
`TokenRefreshBackgroundService.cs` (new, untracked) runs token refresh as a background service — polling frequency and error backoff strategy are unknown.

## Fragile Areas

### 1. Watson.Items.Ingest — Lidl-specific ETL hardcoded
The entire ETL pipeline under `Watson.Items.Ingest/Lidl/` is hardcoded for Lidl receipt format (CSV parsing, filename patterns, downloaders). Adding new retailers requires adding parallel Lidl-style service classes — no plugin/extension point exists yet.

### 2. Sven household model
New models `HouseholdMembership`, `SharedExternalTokenConsent` (untracked) suggest the household feature is mid-development with partially implemented data layer and no tests yet.

### 3. Watson.Nodes change feed
`IChangeFeedService` / `ChangesController` implement a change feed/event stream. If `ChangeFeedQuery` polling is frequent and unbounded, this could create query pressure on the Nodes database.

### 4. Bureau.Frontend.DevRunner filesystem assumption
Dev runner locates the repo root by searching for `app/pnpm-workspace.yaml`. This breaks if the workspace file is renamed or the runner is invoked from an unexpected directory.

## Missing / Incomplete

### 1. Sven Postgres provider — incomplete
`Sven.Data.Postgres` has new `Configurations/`, `Contexts/`, `Health/` folders (untracked) but the original `SvenContextPostgres.cs` and `SvenContextPostgresFactory.cs` were deleted. The Postgres provider is mid-migration.

### 2. No integration tests for Watson.Nodes or Watson.Items
`server/tests/Sven.Tests/` exists but there are no test projects for `Watson.Nodes` or `Watson.Items.Ingest`. Only Sven has a test project.

### 3. No frontend tests visible
No test configuration found in `app/client/` — no Vitest, Jest, or Playwright setup detected.

## Dependencies at Risk

### 1. Sven — self-maintained OIDC implementation
The entire OIDC/OAuth2 stack (token issuance, PKCE, revocation, external providers) is first-party code in `Sven`. This is a high-maintenance dependency — any spec changes or security discoveries require manual fixes.

### 2. Watson.Items.Ingest — retailer API coupling
`LidlDownloaderService` downloads files from Lidl's API. Changes to Lidl's API or file format will break ingest silently until parsing fails.
