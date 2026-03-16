# Roadmap: Sven

## Overview

Sven is ~70% complete as an OAuth 2.0 / OIDC authorization server. This milestone closes the gap:
security hardening on already-present vulnerabilities, three missing grant types (Client Credentials,
Introspection, Token Exchange), the `bureau_features` registration extension that transforms Sven
into a token broker, household identity wiring, and a modular external provider architecture. The
milestone ends with an RFC compliance test suite and documentation that make the codebase readable
alongside the specs.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Security Hardening** - Fix confirmed vulnerabilities before building new features (gap closure in progress) (completed 2026-03-15)
- [x] **Phase 2: Client Credentials Grant** - Add service-to-service token issuance and confidential client auth (completed 2026-03-15)
- [x] **Phase 3: Token Introspection** - Enable resource servers to validate any Sven-issued token (completed 2026-03-16)
- [ ] **Phase 4: end_session Security** - Close the RP-initiated logout open redirect and complete OIDC Session compliance
- [ ] **Phase 5: bureau_features Registration** - Add the client registration extension that gates token exchange
- [ ] **Phase 6: RFC 8693 Token Exchange** - Implement the token broker core: Sven JWTs exchanged for external provider tokens
- [ ] **Phase 7: Household Identity + Modular Providers** - Wire household claims into JWTs and refactor provider architecture
- [ ] **Phase 8: RFC Compliance Test Suite + Documentation** - Consolidate spec-named tests across all RFCs and complete docs

## Phase Details

### Phase 1: Security Hardening
**Goal**: Known security vulnerabilities are eliminated and the codebase passes a basic pre-launch security checklist before any new features are added
**Depends on**: Nothing (first phase)
**Requirements**: SEC-01, SEC-02, SEC-03, SEC-04, CODE-01, CODE-02
**Success Criteria** (what must be TRUE):
  1. Verification codes are generated via `RandomNumberGenerator` and rejected after 5 minutes — verified by test
  2. Requests to `/connect/token`, `/connect/authorize`, and sign-in endpoints return HTTP 429 after exceeding the configured rate limit
  3. Submitting a previously used authorization code returns an error and produces a structured security-event log entry
  4. The AES-256 encryption key is read from an environment variable or secrets manager; `appsettings.json` contains no key material
  5. `Sven.Abstractions`, `Sven.Contracts`, `Sven`, and `Sven.Web` compile with no cross-layer reference violations; all service classes are named `*Service`/`I*Service` and all repository classes are named `*Repository`/`I*Repository`
**Plans**: 6 plans

Plans:
- [x] 01-01-PLAN.md — Wave 0: Create failing test stubs for SEC-01, SEC-02, SEC-03, SEC-04
- [x] 01-02-PLAN.md — Layer boundary enforcement (CODE-01) and naming convention rename (CODE-02)
- [x] 01-03-PLAN.md — Security fixes: verification code TTL (SEC-01), atomic auth code (SEC-03), AES key startup guard (SEC-04)
- [x] 01-04-PLAN.md — Rate limiting middleware on token/authorize/sign-in endpoints (SEC-02)
- [ ] 01-05-PLAN.md — Gap closure: SEC-03 atomic exchange fix + SEC-02 appsettings defaults + middleware ordering
- [ ] 01-06-PLAN.md — Gap closure: CODE-01 layer cleanup + CODE-02 naming completion (delete old provider files, fix SvenTokenProvider)

### Phase 2: Client Credentials Grant
**Goal**: Bureau daemons and service-to-service callers can obtain access tokens without user context using standard confidential client authentication
**Depends on**: Phase 1
**Requirements**: PROT-01
**Success Criteria** (what must be TRUE):
  1. A confidential client can POST to `/connect/token` with `grant_type=client_credentials` using `client_secret_basic` header authentication and receive a valid signed JWT
  2. A confidential client can POST with `client_secret_post` body authentication and receive the same result
  3. The issued token contains no `sub` claim and contains only the scopes registered for that client
  4. An unknown client or wrong secret receives HTTP 401 with an RFC 6749 error response
**Plans**: 5 plans

Plans:
- [ ] 02-01-PLAN.md — Wave 0: Failing test stubs (ClientAuthServiceTests, Rfc6749ClientCredentialsGrantTests, Rfc7591ConfidentialClientRegistrationTests)
- [ ] 02-02-PLAN.md — Wave 1: Data layer — Client.HashedSecret, ClientDb column, migrations (SQL Server + Postgres), repository round-trip
- [ ] 02-03-PLAN.md — Wave 1: AuthConstants + DiscoveryService expansion, IClientAuthService + ClientAuthService, TokenRequest fix
- [ ] 02-04-PLAN.md — Wave 2: HandleClientCredentialsFlow in TokenController, CreateMachineTokenAsync in SvenTokenProvider, DI registration
- [ ] 02-05-PLAN.md — Wave 3: OidcController IAT guard + confidential client registration, ClientService confidential branch

### Phase 3: Token Introspection
**Goal**: Resource servers (Watson.Nodes, Niles.Chores) can validate any Sven-issued token at runtime via a standard introspection endpoint
**Depends on**: Phase 2
**Requirements**: PROT-02
**Success Criteria** (what must be TRUE):
  1. An authenticated resource server can POST to `/oidc/introspect` with a valid active token and receive `{"active": true}` plus standard claims
  2. An expired, revoked, or unknown token returns `{"active": false}` with no additional claims
  3. A caller without valid client credentials is rejected with HTTP 401 before any token data is returned
  4. `introspection_endpoint` appears in the OIDC discovery document at `/.well-known/openid-configuration`
**Plans**: 3 plans

Plans:
- [ ] 03-01-PLAN.md — Wave 1: Failing test stubs (Rfc7662TokenIntrospectionTests, TokenProviderIntrospectTests)
- [ ] 03-02-PLAN.md — Wave 2: Models, constants, ITokenProvider.IntrospectAsync, SvenTokenProvider implementation
- [ ] 03-03-PLAN.md — Wave 2: IntrospectionController, DiscoveryDocument + DiscoveryService wiring, integration tests green

### Phase 4: end_session Security
**Goal**: RP-initiated logout validates the ID token hint and redirect URI with no open-redirect vulnerability; logout completes regardless of redirect URI validity
**Depends on**: Phase 1
**Requirements**: PROT-04
**Success Criteria** (what must be TRUE):
  1. A logout request with an `id_token_hint` issued by a different issuer is rejected before the session is cleared
  2. A logout request with a `post_logout_redirect_uri` not registered for the client completes the logout but does not redirect (returns 200 or redirects to a safe default)
  3. A logout request with a valid `post_logout_redirect_uri` completes and redirects to that URI
  4. Logout with no `post_logout_redirect_uri` completes successfully
**Plans**: TBD

### Phase 5: bureau_features Registration
**Goal**: Client applications can declare which Bureau feature scopes they expose and which external provider scopes each feature requires at registration time; Sven persists and validates these declarations
**Depends on**: Phase 1
**Requirements**: VAULT-01
**Success Criteria** (what must be TRUE):
  1. A client registration request with a valid `bureau_features` field succeeds and the features are returned in the registration response
  2. A registration request with an unknown feature key is rejected with a descriptive error
  3. The registered feature allowlist is retrievable via `IClientProvider` and used to gate token exchange requests in the next phase
  4. EF migrations for `ClientFeatures` and `ClientFeatureExternalRequirements` tables apply cleanly on both Postgres and SQL Server
**Plans**: TBD

### Phase 6: RFC 8693 Token Exchange
**Goal**: Bureau apps can exchange a Sven access token for an external provider access token; the six-step validation chain is fully enforced; household shared token fallback works; every exchange is audit-logged
**Depends on**: Phase 2, Phase 4, Phase 5
**Requirements**: PROT-03, VAULT-03, VAULT-04
**Success Criteria** (what must be TRUE):
  1. A Bureau app holding a valid Sven access token can POST to `/connect/token` with `grant_type=urn:ietf:params:oauth:grant-type:token-exchange` and receive the user's external provider access token for the requested feature
  2. An exchange request where the user has no token for the requested feature but a household member has a shared token succeeds via the fallback path, and `IHouseholdService` confirms current membership from the database
  3. An exchange request that fails any of the six validation steps (invalid signature, wrong issuer, audience mismatch, expired token, inactive user, feature not in client's `bureau_features` allowlist) returns the appropriate RFC 8693 error
  4. Every token exchange attempt — success or failure — produces a structured audit log entry containing `client_id`, `user_id`, `feature`, `provider`, `source_user_id`, `timestamp`, and `result`
  5. Sven access tokens for household members include `household_id` and `household_role` JWT claims sourced from the live database
**Plans**: TBD

### Phase 7: Household Identity + Modular Providers
**Goal**: External provider token refresh is handled by self-contained provider modules with no switch statements in core code; household JWT claims are live-wired; Microsoft provider is fully tested
**Depends on**: Phase 6
**Requirements**: VAULT-02
**Success Criteria** (what must be TRUE):
  1. Adding a new external provider requires only a new `IExternalProviderModule` implementation — no changes to `ExternalTokenRefresher` or any other core class
  2. `ExternalTokenRefresher` contains no provider-specific switch statement or conditional chains; provider selection is done via keyed DI services
  3. Microsoft external login sign-in and token refresh paths pass integration tests
**Plans**: TBD

### Phase 8: RFC Compliance Test Suite + Documentation
**Goal**: All implemented RFCs have spec-named test classes where a failing test means a spec violation; the codebase is fully documented for external readers and future maintainers
**Depends on**: Phase 7
**Requirements**: TEST-01, TEST-02, TEST-03, TEST-04, DOCS-01, DOCS-02, DOCS-03, DOCS-04, CODE-03, CODE-04
**Success Criteria** (what must be TRUE):
  1. A test class named after each implemented RFC exists (e.g., `Rfc6749AuthorizationCodeGrantTests`, `Rfc8693TokenExchangeTests`); every MUST/SHALL clause in scope has at least one test
  2. Multi-account and multi-provider linking scenarios (same provider, different emails) are covered by tests that verify correct sign-in routing
  3. Token vault paths — storage, background refresh, on-demand fallback, and revocation — are covered by the test suite
  4. Every supported grant type has an end-to-end integration test using `WebApplicationFactory` that passes on both Postgres and SQL Server
  5. Service and controller methods carry XML doc comments referencing the relevant RFC section; token issuance, exchange, and error events are logged via `ILogger` with structured message templates
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Security Hardening | 6/6 | Complete   | 2026-03-15 |
| 2. Client Credentials Grant | 5/5 | Complete   | 2026-03-15 |
| 3. Token Introspection | 3/3 | Complete   | 2026-03-16 |
| 4. end_session Security | 0/TBD | Not started | - |
| 5. bureau_features Registration | 0/TBD | Not started | - |
| 6. RFC 8693 Token Exchange | 0/TBD | Not started | - |
| 7. Household Identity + Modular Providers | 0/TBD | Not started | - |
| 8. RFC Compliance Test Suite + Documentation | 0/TBD | Not started | - |
