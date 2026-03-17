---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: planning
stopped_at: Completed 05-bureau-features-registration 05-03-PLAN.md
last_updated: "2026-03-17T13:35:08.944Z"
last_activity: 2026-03-15 — Completed SEC-01/SEC-03/SEC-04 security fixes (TTL, atomic auth code, AES key guard)
progress:
  total_phases: 8
  completed_phases: 5
  total_plans: 22
  completed_plans: 22
---

---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: planning
stopped_at: Completed 04-end-session-security 04-03-PLAN.md
last_updated: "2026-03-16T10:50:19.660Z"
last_activity: 2026-03-15 — Completed SEC-01/SEC-03/SEC-04 security fixes (TTL, atomic auth code, AES key guard)
progress:
  total_phases: 8
  completed_phases: 3
  total_plans: 19
  completed_plans: 18
  percent: 100
---

---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: planning
stopped_at: Completed 03-token-introspection 03-02-PLAN.md
last_updated: "2026-03-16T06:21:59.709Z"
last_activity: 2026-03-15 — Completed SEC-01/SEC-03/SEC-04 security fixes (TTL, atomic auth code, AES key guard)
progress:
  [██████████] 100%
  completed_phases: 2
  total_plans: 14
  completed_plans: 13
  percent: 100
---

---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: planning
stopped_at: Completed 01-security-hardening 01-03-PLAN.md
last_updated: "2026-03-15T12:00:00Z"
last_activity: 2026-03-15 — Completed CODE-01/CODE-02 layer boundary and naming convention refactor
progress:
  [██████████] 100%
  completed_phases: 0
  total_plans: 4
  completed_plans: 3
  percent: 75
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-15)

**Core value:** Bureau apps authenticate against one system (Sven) and request external tokens from one system (Sven) — no Bureau app implements provider-specific OAuth, and no external secret leaves Sven.
**Current focus:** Phase 1 - Security Hardening

## Current Position

Phase: 1 of 8 (Security Hardening)
Plan: 3 of 4 in current phase
Status: In progress
Last activity: 2026-03-15 — Completed SEC-01/SEC-03/SEC-04 security fixes (TTL, atomic auth code, AES key guard)

Progress: [███████░░░] 75%

## Performance Metrics

**Velocity:**
- Total plans completed: 0
- Average duration: -
- Total execution time: 0 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| - | - | - | - |

**Recent Trend:**
- Last 5 plans: none yet
- Trend: -

*Updated after each plan completion*
| Phase 01-security-hardening P01 | 8 | 2 tasks | 4 files |
| Phase 01-security-hardening P02 | 15 | 2 tasks | 28 files |
| Phase 01-security-hardening P03 | 2 sessions | 2 tasks | 11 files |
| Phase 01-security-hardening P04 | 90 | 2 tasks | 8 files |
| Phase 01-security-hardening P05 | 15 | 2 tasks | 3 files |
| Phase 01-security-hardening P06 | 25 | 2 tasks | 25 files |
| Phase 02-client-credentials-grant P01 | 7 | 3 tasks | 4 files |
| Phase 02-client-credentials-grant P02 | 10 | 2 tasks | 8 files |
| Phase 02-client-credentials-grant P03 | 15 | 2 tasks | 5 files |
| Phase 02-client-credentials-grant P04 | 40 | 2 tasks | 8 files |
| Phase 02-client-credentials-grant P05 | 12 | 2 tasks | 4 files |
| Phase 03-token-introspection P01 | 6 | 2 tasks | 3 files |
| Phase 03-token-introspection P02 | 10 | 2 tasks | 6 files |
| Phase 03-token-introspection P03 | 25 | 2 tasks | 7 files |
| Phase 03-token-introspection P04 | 8 | 1 tasks | 1 files |
| Phase 04-end-session-security P01 | 8 | 1 tasks | 1 files |
| Phase 04-end-session-security P02 | 3 | 2 tasks | 6 files |
| Phase 04-end-session-security P03 | 10 | 2 tasks | 6 files |
| Phase 04-end-session-security P04-04 | 15 | 2 tasks | 5 files |
| Phase 05-bureau-features-registration P02 | 20 | 2 tasks | 12 files |
| Phase 05-bureau-features-registration P01 | 5 | 1 tasks | 5 files |
| Phase 05-bureau-features-registration P03 | 7 | 2 tasks | 9 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Roadmap]: CODE-01 and CODE-02 (layer cleanup + naming conventions) placed in Phase 1 — foundational work that must precede all new feature development
- [Roadmap]: Phase 4 (end_session) depends on Phase 1 only, not Phase 2/3 — can execute in parallel with 2/3 if desired
- [Roadmap]: Phase 6 depends on Phases 2, 4, and 5 — latest dependency gate in the roadmap
- [Research]: Do NOT adopt OpenIddict or Duende IdentityServer; continue custom implementation
- [Phase 01-security-hardening]: Assert.Fail used instead of SkipException in test stubs — xUnit 2.5.3 does not expose SkipException (xUnit v3 API only)
- [Phase 01-security-hardening]: RateLimitingTests uses IClassFixture<SvenWebAppFactory>; CreateClient called inside test body as placeholder for implementers to configure low-threshold rate limits via WithWebHostBuilder
- [Phase 01-security-hardening P02]: IAuthCodeService placed in Sven (not Sven.Abstractions) — AuthCode/OAuthRequest types are Sven-local; Abstractions cannot reference Sven
- [Phase 01-security-hardening P02]: IUserService.GetUserByIdAsync added so UserInfoController avoids injecting internal IUserRepository
- [Phase 01-security-hardening P02]: Sven.Data.Postgres and Sven.Data.SqlServer already referenced Sven.csproj — no csproj changes needed for CODE-01
- [Phase 01-security-hardening P03]: EncryptionKeyStartupFilter added alongside ValidateOnStart guard on EncryptionKeysOptions; both fire at startup ensuring dual-layer key validation
- [Phase 01-security-hardening P03]: ExchangeCodeAsync uses RemoveAsync on InMemoryStore (ConcurrentDictionary.TryRemove) which is atomic; TryRemoveAtomic added as explicit public method for direct callers
- [Phase 01-security-hardening P03]: AesEncryptorTests use ThrowsAny<Exception> + ContainsGuardException helper because OptionsValidationException fires before IStartupFilter in minimal hosting
- [Phase 01-security-hardening]: IP-based partitioning chosen for rate limiting on all three auth endpoints; client_id partitioning deferred, documented via _note in appsettings.json
- [Phase 01-security-hardening]: QueueLimit=0 on all rate-limit policies — reject immediately on exhaustion, no queuing to avoid latency spikes under attack
- [Phase 01-security-hardening]: AuthCodeService constructor accepts InMemoryStore<string, AuthCode> directly; both IStore and concrete type registered as singletons sharing one instance
- [Phase 01-security-hardening]: GetAuthCodeAsync and ClearAuthCodeAsync retained on IAuthCodeService — still used by TokenController two-step flow
- [Phase 01-security-hardening]: MapRazorPages and MapControllers moved after UseRateLimiter + UseAuthorization to ensure rate-limit policies apply to Razor Pages
- [Phase 01-security-hardening]: IExternalProviderRegistry kept public in Sven/Services — Sven.Web page models inject it directly; making internal would break eight files
- [Phase 01-security-hardening]: GetUserByUsernameAsync added to IUserService — UserClaimsProvider needed username lookup after IUserStore removal
- [Phase 01-security-hardening]: SvenServiceCollectionExtensions.AddSvenCore is single DI registration point for all Sven-internal services including IStore variants, IExternalTokenRefresher
- [Phase 02-client-credentials-grant]: Assert.Fail used for stubs (same pattern as Phase 1) — xUnit 2.5.3 has no SkipException
- [Phase 02-client-credentials-grant]: Integration test stubs are synchronous void Facts — no async needed since body is Assert.Fail only
- [Phase 02-client-credentials-grant]: ClientDb in Sven.Data/Models/ is the active EF entity; Sven/Data/Models/ClientDb.cs is excluded from Sven.csproj compile
- [Phase 02-client-credentials-grant]: HashedSecret stored as first-class column (not in ClientAddendum JSON) — enables direct SQL indexing and avoids JSON deserialization for auth path
- [Phase 02-client-credentials-grant]: IClientAuthService is internal to Sven — it consumes HttpRequest (ASP.NET Core type), unsuitable for Sven.Abstractions
- [Phase 02-client-credentials-grant]: request.HasFormContentType guard added in ClientAuthService before accessing Form to prevent InvalidOperationException
- [Phase 02-client-credentials-grant]: IClientAuthService resolved via HttpContext.RequestServices in TokenController — internal type cannot be a public constructor parameter (CS0051); runtime resolution avoids accessibility violation
- [Phase 02-client-credentials-grant]: ITokenProvider registered via factory lambda inside AddSvenCore() — SvenTokenProvider constructor is internal and uses internal IStore<>; reflection-based DI fails at runtime
- [Phase 02-client-credentials-grant]: Machine token aud = issuer (not Audience) — RFC 9068 §2.2: for client_credentials aud identifies the authorization server itself
- [Phase 02-client-credentials-grant]: EF in-memory: ApplyConfigurationsFromAssembly fails for open-generic IEntityTypeConfiguration<T>; test context must call Configure() directly on the concrete type
- [Phase 02-client-credentials-grant]: IConfiguration injected into OidcController via constructor for IAT (Sven:InitialAccessToken) access
- [Phase 02-client-credentials-grant]: IAT guard runs before all other registration checks; 501 if key unconfigured, 401 on wrong token
- [Phase 02-client-credentials-grant]: Raw client secret generated in CreateClientAsync, stored on client.ClientSecret (ephemeral), never persisted — only HashedSecret stored in DB
- [Phase 03-token-introspection]: Endpoints.Oidc.Introspect constant added in Plan 01 so integration test file compiles immediately without string literals
- [Phase 03-token-introspection]: xUnit [Fact] methods cannot accept CancellationToken parameters — removed from test method signatures
- [Phase 03-token-introspection]: Unit stubs compile without ITokenProvider.IntrospectAsync — pure Assert.Fail bodies with no production type references
- [Phase 03-token-introspection]: IntrospectionResponse placed in Sven.Abstractions/Models — ITokenProvider cannot reference Sven project types
- [Phase 03-token-introspection]: IntrospectAsync uses Task.FromResult (no I/O path) — JWT parsing is CPU-only, no awaitable operations needed
- [Phase 03-token-introspection]: IntrospectionController routes under Endpoints.Oidc.Base; IClientAuthService resolved via HttpContext.RequestServices (CS0051 constraint)
- [Phase 03-token-introspection]: SvenTokenProvider.IntrospectAsync uses TokenValidationParameters with IssuerSigningKey for full signature validation; ReadJwtToken was replaced to prevent wrong-key tokens returning active:true
- [Phase 03-token-introspection]: Sven.Contracts has parallel Endpoints and DiscoveryDocument definitions that must be kept in sync with Sven project equivalents
- [Phase 03-token-introspection]: Expired token integration test needs no time-travel: manually construct JWT with exp in past signed with real server key; ValidateLifetime=true in TokenValidationParameters rejects it at introspect time
- [Phase 04-end-session-security]: Synchronous void [Fact] stubs (not async) used for Assert.Fail stubs — xUnit 2.5.3 pattern; AllowAutoRedirect=false on HttpClient for 302 verification
- [Phase 04-end-session-security]: PostLogoutRedirectUris stored as nullable JSON text column (same pattern as Contacts) — not structured relational rows, appropriate for atomic client record read/write
- [Phase 04-end-session-security]: Postgres migration uses 'text' type, SqlServer uses 'nvarchar(max)' for unbounded List<string> JSON — consistent with existing RedirectUris/Contacts migrations
- [Phase 04-end-session-security]: Sven.Contracts references Sven.Abstractions so AuthConstants.FieldNames.PostLogoutRedirectUris constant is used in both registration request models
- [Phase 04-end-session-security]: ClientRegistrationResponse inherits ClientRegistrationRequest — adding PostLogoutRedirectUris to request covers response without separate update
- [Phase 04-end-session-security]: Endpoints.Connect.EndSession added to Sven/Configurations/Endpoints.cs (mirrors Sven.Contracts) — DiscoveryService cannot reference Sven.Contracts
- [Phase 04-end-session-security]: ValidateLifetime=false in EndSessionController TokenValidationParameters — OIDC Session §5 requires expired id_token_hint to be accepted
- [Phase 04-end-session-security]: IClientService injected via EndSessionController constructor (not RequestServices) — public interface, no CS0051 constraint
- [Phase 05-bureau-features-registration]: ClientFeatureDb/ClientFeatureExternalRequirementDb are public (consistent with ClientDb); relationship defined from ClientFeatureDb side only to avoid shadow FK columns; base configs omit ToTable() for in-memory test compatibility
- [Phase 05-bureau-features-registration]: BureauFeaturesRegistrationTests uses synchronous void [Fact] Assert.Fail stubs — xUnit 2.5.3 pattern
- [Phase 05-bureau-features-registration]: ClientFeatureDb/ClientFeatureExternalRequirementDb changed from internal to public — public navigation property on public ClientDb requires public type
- [Phase 05-bureau-features-registration]: ToTable() removed from Sven.Data base type configs — relational extension not in EF Core base package; convention naming used
- [Phase 05-bureau-features-registration]: ProblemCodes.Request.InvalidPayload used for bureau_features validation errors — maps to HTTP 400
- [Phase 05-bureau-features-registration]: IExternalProviderRegistry injected into ClientService for scope key validation — registry is a singleton always available via AddSvenAuthentication
- [Phase 05-bureau-features-registration]: Two-save EF pattern in StoreAsync: first save persists ClientDb parent to get Id, second save persists ClientFeatureDb children

### Pending Todos

None yet.

### Blockers/Concerns

- [Phase 5 planning]: `bureau_features` schema design (ClientFeatures / ClientFeatureExternalRequirements tables) needs to be worked out against FeatureKeys.cs in Bureau.Primitives before planning can begin
- [Phase 6 planning]: Token Exchange six-step validation chain needs detailed task breakdown against SVEN.md §8.3 before planning can begin
- [Phase 7]: Microsoft provider integration tests may require an Azure AD test app registration — confirm test environment approach before Phase 7

## Session Continuity

Last session: 2026-03-17T13:35:08.940Z
Stopped at: Completed 05-bureau-features-registration 05-03-PLAN.md
Resume file: None
