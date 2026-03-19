# Sven — Identity Provider & Token Broker

Sven is Bureau's self-hosted OAuth 2.0 / OpenID Connect server. It has three distinct but
complementary responsibilities:

1. **Identity Provider (IdP)** — authenticates users for Bureau apps (bureau-web,
   niles-chores-web, mobile) via local credentials or external OAuth providers (Google,
   Microsoft). Issues signed JWT access tokens and ID tokens to client applications.

2. **Token Vault & Broker** — stores the external OAuth tokens users have granted for Bureau
   features (e.g., Google Calendar, OneDrive), keeps them fresh via a background refresh
   process, and dispenses them to Bureau apps on demand via a secure token exchange endpoint.
   Apps never obtain refresh tokens for external providers; they ask Sven for a current access
   token and Sven handles the rest.

3. **Household identity** — users can be grouped into a household. The household identity is
   carried in JWTs so Bureau APIs can scope data (e.g., a shared chore list) to the household.
   Household members can also consent to share external tokens with other members for specific
   features (e.g., one member's Google Calendar token usable by apps on behalf of others).

Sven is designed to be **internet-exposed**: it handles mobile app auth (system browser + PKCE),
web app auth, and external provider callbacks.

---

## 1. Design Principles

- **RFC-first** — every protocol behaviour MUST conform to the referenced RFC or OIDC
  specification. Where a spec offers a choice, the most secure option is taken.
- **Least privilege** — apps receive only the external tokens they are explicitly authorised
  to request, and only for the user whose Sven JWT they present.
- **Encrypted at rest** — all external OAuth tokens are AES-256 encrypted in the database.
  External refresh tokens are never returned to client apps.
- **Public-facing safe** — PKCE required for all flows, no implicit flow, HTTPS enforced in
  production, explicit CORS allowlist in non-dev environments.
- **Single identity** — a user may link multiple external accounts (Google, Microsoft, …).
  All are unified under one Sven account. Bureau apps authenticate against Sven only.

---

## 2. RFC & Specification Compliance

| Specification | Description | Status |
|--------------|-------------|--------|
| RFC 6749 | OAuth 2.0 Authorization Framework | ✅ Auth Code flow |
| RFC 6750 | Bearer Token Usage | ✅ |
| RFC 6819 | OAuth 2.0 Threat Model & Security | 🟡 Partial — see §9 |
| RFC 7009 | Token Revocation | ✅ |
| RFC 7517 | JSON Web Key (JWK) | ✅ |
| RFC 7518 | JSON Web Algorithms | ✅ RS256 only |
| RFC 7519 | JSON Web Token (JWT) | ✅ |
| RFC 7591 | Dynamic Client Registration | ✅ |
| RFC 7636 | Proof Key for Code Exchange (PKCE) | ✅ S256 required |
| RFC 7662 | Token Introspection | ⬜ Planned |
| RFC 8252 | OAuth 2.0 for Native Apps | ✅ PKCE required; custom URI schemes supported |
| RFC 8693 | OAuth 2.0 Token Exchange | ⬜ Planned (token broker, §7) |
| OpenID Connect Core 1.0 | Core OIDC | 🟡 Partial — UserInfo and end_session pending |
| OpenID Connect Discovery 1.0 | Discovery document & JWKS | ✅ |
| OpenID Connect Session Mgmt 1.0 | RP-initiated logout | ⬜ Planned |
| OpenID Connect Dynamic Registration 1.0 | Client registration | ✅ via RFC 7591 |

Implicit flow, Resource Owner Password Credentials, and Device Code flow are intentionally
not supported.

---

## 3. Supported OAuth Flows

### 3.1 Authorization Code + PKCE (RFC 7636)

The only supported authorization flow for all client applications.

- Code challenge method: `S256` required (`plain` rejected)
- Redirect URI: exact match, no wildcards, no open redirectors
- `state` parameter: required by all clients; validated end-to-end
- `nonce`: bound into the ID token when provided

### 3.2 Refresh Tokens

Issued when the `offline_access` scope is requested.

- Stored in the database (not in-memory — survives restarts)
- Rotation on every use: old token is invalidated immediately upon use
- Scope cannot be widened beyond the originally granted set
- Bound to `client_id` + `redirect_uri` + `scope`

### 3.3 Token Exchange (RFC 8693) — Planned

Bureau apps exchange a Sven access token for an external provider token. See §7.

---

## 4. Identity Hub

### 4.1 Local accounts

Users register with email + password. Passwords are hashed with bcrypt (work factor 12).

### 4.2 External providers

Users link one or more external OAuth provider accounts to their single Sven account. They
can then sign in to Sven (and therefore to all Bureau apps) using any linked provider.

| Provider | Protocol | Scopes requested on link | Status |
|----------|----------|--------------------------|--------|
| Google | OAuth 2.0 + OIDC | `openid email profile` (+ feature scopes on demand) | ✅ |
| Microsoft | OAuth 2.0 + OIDC | `openid email profile offline_access` | 🟡 Configured, needs testing |
| Facebook | OAuth 2.0 | `email public_profile` | ⬜ Planned |

**Link flow:**

1. User initiates link from Sven account management page.
2. Sven redirects to the external provider consent screen with the base identity scopes.
3. On callback, Sven creates or updates the `LinkedIdentity` record and stores the OAuth tokens
   in `UserExternalTokens` (encrypted).
4. The user can now sign in to Sven using that provider.
5. Additional external scopes (for specific features) are requested incrementally — see §5.3.

**Sign-in with a linked provider:**

When a user authenticates via Google, Sven receives the Google tokens, looks up the matching
Sven account by `LinkedIdentity`, and issues its own JWT. The Google tokens are refreshed and
stored; the Bureau app receiving Sven's JWT never sees them.

---

## 5. Feature–Scope Mapping

Bureau features (defined in `Bureau.Primitives/Features/FeatureKeys.cs` on the server,
mirrored in `app/client/apps/bureau-web/src/features/featureKeys.ts` on the frontend) have
two orthogonal concerns:

- **Bureau feature scope** (e.g., `watson.calendar`) — OAuth 2.0 scope issued by Sven in
  JWTs; controls what a user can do in Bureau apps (`[RequireFeature]`).
- **External provider scope** (e.g., `https://www.googleapis.com/auth/calendar`) — OAuth 2.0
  scope issued by Google/Microsoft; controls what external APIs can be called on the user's
  behalf via the token vault.

Some features need only a Bureau scope (e.g., `watson.nodes.crud` is purely internal). Others
need a Bureau scope **and** external provider scopes — a Calendar feature needs `watson.calendar`
in the Sven JWT to enter the Bureau app, and also a valid Google Calendar access token in the
vault to call Google's API.

### 5.1 App-declared feature registry

**Apps declare their own feature requirements at registration time.** When a Bureau app
registers with Sven (see §8 — `POST /oidc/register`), it includes a `bureau_features` metadata
extension (RFC 7591 allows additional registration fields) describing:

- Which Bureau feature scopes it exposes
- For each feature that needs external access: which provider scopes are required

Example registration payload fragment:

```json
{
  "client_name": "watson",
  "bureau_features": [
    {
      "key": "watson.nodes",
      "external_requirements": []
    },
    {
      "key": "watson.calendar",
      "external_requirements": [
        {
          "provider": "google",
          "scopes": [
            "https://www.googleapis.com/auth/calendar.readonly",
            "https://www.googleapis.com/auth/calendar.events"
          ]
        },
        {
          "provider": "microsoft",
          "scopes": ["Calendars.ReadWrite"]
        }
      ]
    },
    {
      "key": "watson.mail",
      "external_requirements": [
        { "provider": "google", "scopes": ["https://www.googleapis.com/auth/gmail.readonly"] },
        { "provider": "microsoft", "scopes": ["Mail.Read"] }
      ]
    }
  ]
}
```

Sven stores this in the `ClientFeatures` / `ClientFeatureExternalRequirements` tables and
builds the feature registry from all registered clients. The Sven admin UI shows the aggregate
view; individual feature requirements come from the apps, not from Sven itself.

This means: adding a new external integration to Watson requires only a Watson re-registration
(or a registration update) — no Sven code changes.

### 5.2 User authorisation (incremental consent)

A user explicitly authorises each feature that requires external scopes via the Sven account
management UI:

1. User navigates to **Connected Services → Authorize Feature**.
2. Sven looks up the feature's `external_requirements` from the client registry.
3. If the user's stored token for that provider doesn't include those scopes, Sven redirects
   to the provider's consent screen requesting the additional scopes (incremental OAuth consent).
4. On callback, Sven merges the new scopes into the stored token and marks the feature as
   authorised for that user.

Until a feature is authorised, Sven will refuse token exchange requests for it.

### 5.3 Client token-request allowlist

An app can only request external tokens for features it **itself declared** in its registration.
Even if a user has authorised `watson.calendar`, a different Bureau app (e.g., `niles-chores-web`)
cannot request that token — it's not in its `bureau_features` list.

---

## 6. Token Vault

### 6.1 Storage

External tokens are persisted in `UserExternalTokens`:

| Column | Type | Notes |
|--------|------|-------|
| `UserId` | uuid | Sven user identifier |
| `Provider` | string | `google`, `microsoft`, … |
| `Scopes` | string | Space-separated scope string as granted |
| `AccessToken` | string | AES-256-GCM encrypted |
| `RefreshToken` | string? | AES-256-GCM encrypted; nullable (some providers don't issue one) |
| `ExpiresAt` | datetime | UTC expiry of the access token |
| `LinkedAt` | datetime | When the connection was established |
| `LastRefreshedAt` | datetime? | When the access token was last refreshed |

One row per user–provider combination. If a user grants additional scopes for the same provider
(incremental consent), the row is updated in place.

### 6.2 Token refresh (background process + on-demand fallback)

**Background refresh** — `TokenRefreshBackgroundService` (an ASP.NET Core `BackgroundService`)
runs on a configurable interval (default: every 5 minutes). It queries the database for all
`UserExternalTokens` where `ExpiresAt < UtcNow + RefreshLeadTime` (default lead time: 15 minutes)
and refreshes them proactively. Refreshed tokens are persisted immediately. If a refresh fails
(revoked, expired refresh token), the row is marked `RequiresReauthorisation = true` and an
audit log entry is written.

**On-demand fallback** — when a token exchange request arrives and the token is still marked
as expired or near-expired (e.g., the background service was temporarily down), Sven refreshes
inline before returning. This ensures correctness even under background service failure.

**Configuration:**

```json
{
  "TokenVault": {
    "RefreshIntervalSeconds": 300,
    "RefreshLeadTimeMinutes": 15
  }
}
```

If the provider issues a new refresh token (rolling refresh), that is persisted as well.
External refresh tokens are never returned to client apps under any circumstances.

---

## 7. Households & Sharing

> **Implementation status: placeholder.** The data model is defined and the `household_id`
> claim is issued in JWTs. Invite flow, sharing UI, and conflict resolution are not yet
> implemented. The structure is intentionally minimal so full sharing can be layered on later
> without schema changes.

### 7.1 Concept

A **household** is a named group of Sven users (typically a family or flat-share). Users can
belong to at most one household. Household membership unlocks two capabilities:

1. **Scoped data in Bureau apps** — APIs receive `household_id` in the JWT and can use it to
   share data across members (e.g., a single chore list, a shared node graph). This is
   app-level logic; Sven's role is only to carry the claim.

2. **Shared external tokens** — a household member can consent to share their external provider
   token for a specific feature with other members. When a token exchange is requested for a
   user who has no token for a feature, Sven checks whether a household member has shared one.

### 7.2 Data model

```
Household
  Id            uuid  PK
  Name          string
  CreatedAt     datetime

HouseholdMember
  HouseholdId   uuid  FK → Household.Id
  UserId        uuid  FK → SvenUser.SubjectId
  Role          enum  Owner | Member
  JoinedAt      datetime
  (PK: HouseholdId + UserId)

SharedExternalToken
  Id            uuid  PK
  HouseholdId   uuid  FK → Household.Id
  OwnerUserId   uuid  FK → SvenUser.SubjectId  (who granted consent)
  Provider      string                          ("google", "microsoft")
  FeatureKey    string                          (e.g., "watson.calendar")
  SharedAt      datetime
  RevokedAt     datetime?                       (null = still active)
```

`SharedExternalToken` means: *"OwnerUserId consents to Sven using their {Provider} token for
{FeatureKey} on behalf of any member of {HouseholdId}."* The underlying token row in
`UserExternalTokens` is still owned by `OwnerUserId`; sharing is a pointer, not a copy.

### 7.3 JWT claims

When a user is a household member, the Sven access token includes:

```json
{
  "sub": "<user-id>",
  "household_id": "<household-id>",
  "household_role": "member"
}
```

Bureau APIs use `household_id` to scope shared resources. No `household_id` claim = solo user.

### 7.4 Token exchange with sharing

During token exchange (§8), if the requesting user has no authorised token for the requested
feature, Sven checks `SharedExternalToken` for an active shared token from a household member.
If found, Sven uses the owner's stored token to fulfil the request, with the exchange logged
against both users.

### 7.5 What is NOT yet implemented

- Household creation, invite, and accept flow
- Sharing consent UI (the Razor page to create/revoke `SharedExternalToken` rows)
- Per-feature sharing granularity in the UI
- Conflict resolution when multiple household members share the same feature/provider

These can be implemented without any schema changes.

---

## 8. Token Broker (RFC 8693 Token Exchange)

Bureau apps request external tokens by presenting their own Sven JWT and specifying which
feature they need.

### 8.1 Request

```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=urn:ietf:params:oauth:grant-type:token-exchange
&client_id=<bureau-app-client-id>
&subject_token=<Sven access token of the current user>
&subject_token_type=urn:ietf:params:oauth:token-type:access_token
&resource=google
&scope=watson.calendar
```

### 8.2 Response

```json
{
  "access_token": "<Google Calendar access token>",
  "token_type": "Bearer",
  "expires_in": 3540,
  "issued_token_type": "urn:ietf:params:oauth:token-type:access_token"
}
```

### 8.3 Validation steps

Sven performs all of the following before dispensing a token:

1. `client_id` identifies a registered, active Bureau client.
2. The client's `bureau_features` list includes the requested scope (e.g., `watson.calendar`).
3. `subject_token` is a valid, non-expired Sven access token.
4. The user bound to `subject_token` has the requested Bureau feature scope in their Sven claims.
5. The user has authorised the feature for the requested provider **OR** a household member has
   a shared token for this feature/provider (see §7.4).
6. The stored external token (or its refreshed replacement) has the required provider scopes.

### 8.4 Security constraints

- Apps receive **access tokens only** — never the external refresh token.
- Each exchange is audit-logged: `client_id`, `user_id`, `feature`, `provider`, `timestamp`,
  `source_user_id` (differs from `user_id` when fulfilled via a household shared token).
- Rate limited per `client_id` (planned).

---

## 9. Endpoints

| Endpoint | Method | Spec | Purpose |
|----------|--------|------|---------|
| `/connect/authorize` | GET | RFC 6749, RFC 7636 | Authorization endpoint |
| `/connect/token` | POST | RFC 6749, RFC 8693 | Token issuance + token exchange |
| `/connect/revocation` | POST | RFC 7009 | Token revocation |
| `/connect/userinfo` | GET | OIDC Core §5.3 | UserInfo endpoint |
| `/connect/endsession` | GET | OIDC Session §5 | RP-initiated logout |
| `/.well-known/openid-configuration` | GET | OIDC Discovery | Discovery document |
| `/.well-known/jwks.json` | GET | RFC 7517 | Public key set |
| `/oidc/register` | POST | RFC 7591 | Dynamic client registration |
| `/oidc/introspect` | POST | RFC 7662 | Token introspection (planned) |
| `/connect/signin` | GET / POST | — | Sign-in Razor page |
| `/connect/signup` | GET / POST | — | Sign-up Razor page |
| `/account/*` | GET / POST | — | Account & connected services management |

---

## 10. Security Model

### 9.1 Token security

- All JWTs signed with **RS256** (RSA-2048). Key generated at startup; never written to disk.
  Rotated on each restart (clients re-fetch JWKS automatically via discovery).
- External tokens **AES-256-GCM encrypted** at rest; encryption key in `appsettings` / secrets.
- Authorization codes: single-use, max 5-minute TTL, bound to PKCE verifier.
- Sven refresh tokens: single-use rotation, stored in database, bound to `client_id` +
  `redirect_uri` + `scope`.

### 9.2 Public-facing hardening

- PKCE `S256` required for all clients — `plain` rejected.
- No implicit flow.
- `state` parameter enforced on all authorization requests.
- HTTPS required in production (enforced at reverse proxy).
- CORS: `*` (all localhost) in dev; explicit allowlist in test/prod.
- Redirect URIs: exact match only; registered at client creation.

### 9.3 Known gaps (to be addressed)

| Issue | Severity | Fix |
|-------|----------|-----|
| Verification codes use `System.Random` (not cryptographic) | High | Replace with `RandomNumberGenerator` |
| Verification codes have no expiry | Medium | Enforce 5-minute TTL |
| No rate limiting on `/connect/token` and `/connect/authorize` | Medium | Add per-IP and per-client rate limiting |
| Refresh tokens stored in-memory (lost on restart) | High | Migrated to database (done for Sven refresh tokens; in-memory stores removed) |
| No audit log of token issuance / exchange | Medium | Structured log entries with correlation IDs |

---

## 11. Deployment

Sven is a standalone ASP.NET Core application. Sign-in and account management UI are Razor
Pages served by the same process — this is **required**: the sign-in page must be on the same
domain as the OIDC endpoints so that ASP.NET Cookie Authentication sessions work correctly.

```
auth.bureau.home → Sven (port 5100)
  /connect/*                  — OAuth 2.0 / OIDC protocol endpoints
  /oidc/*                     — Registration & introspection
  /.well-known/*              — Discovery & JWKS
  /connect/signin             — Sign-in Razor page
  /connect/signup             — Sign-up Razor page
  /account/*                  — Account management & connected services
```

External provider callbacks must resolve to `auth.bureau.home`:
- `auth.bureau.home/signin-google`
- `auth.bureau.home/signin-microsoft`

These URIs must be registered in each provider's developer console.

### Configuration

```json
{
  "Jwt": {
    "Issuer": "https://auth.bureau.home",
    "Audience": "bureau",
    "IdTokenLifetime": "00:05:00",
    "AccessTokenLifetime": "01:00:00",
    "RefreshTokenLifetime": "30.00:00:00"
  },
  "Auth": {
    "AuthorizationCodeLifetime": "00:05:00"
  },
  "Encrypt": {
    "SymKey": "<44-char base64 AES-256 key>"
  },
  "Google": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "Microsoft": {
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

---

## 12. Relationship to Bureau Feature Scopes

To be explicit about how the two scope systems interact:

```
User authenticates to Sven
  └─ Sven JWT contains Bureau feature scopes: ["watson.nodes", "niles.chores", "watson.calendar"]
       └─ Bureau apps use these to control UI visibility and API access ([RequireFeature])

App needs to call Google Calendar for that user
  └─ App calls POST /connect/token (token exchange, RFC 8693)
       subject_token = user's Sven JWT
       scope = watson.calendar
       resource = google
  └─ Sven validates, fetches/refreshes stored Google token, returns access token
       └─ App calls Google Calendar API directly with that token
```

The user interacts with only one auth system (Sven). External provider complexity is entirely
hidden inside Sven. Bureau apps never implement provider-specific OAuth flows.
