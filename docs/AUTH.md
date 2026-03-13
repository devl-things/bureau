# Bureau Authentication

---

## 1. Auth modes

| Mode | Use case | How it works |
|------|----------|--------------|
| `dev` | Local development | No login flow. Dev token auto-accepted by API. All features enabled. |
| `oidc` | Test + prod | Full OIDC authorization code + PKCE flow via Sven. |

Set in `config.json` (`auth.mode`) on the frontend and in `appsettings.json` (`Auth:Mode`) on each API.

---

## 2. Dev mode

### Frontend

`DevAuthProvider` (`src/auth/DevAuthProvider.ts`):
- `user = { name: "dev-user" }`
- `getAccessTokenAsync()` returns `"dev-token"`
- `featureScopes` = all `FeatureKeys` (full access)

No redirect, no OIDC server needed.

### Backend

`DevApiTokenAuthenticationHandler` (in `Bureau.Server.Hosting`):
- Accepts `Authorization: Bearer dev-token`
- `DevPrincipalFactory` creates a `ClaimsPrincipal` with name, email, role claims, and
  feature `scope` claims from `Auth:Dev:Features` config

```json
// appsettings.Development.json
{
  "Auth": {
    "Mode": "Dev",
    "Dev": {
      "UserName": "dev",
      "Email": "dev@local",
      "ApiToken": "dev-token",
      "Roles": ["Admin"],
      "Features": []
    }
  }
}
```

`Features: []` → inject all `FeatureKeys.AllKeys` (full access).
`Features: ["niles.chores"]` → inject only the listed scopes (simulate restricted user).

---

## 3. OIDC mode (Sven — not yet merged)

Sven is a self-hosted OAuth 2.0 + OIDC server living in `feature/sven` of this repo.

**Capabilities:**
- Authorization code + PKCE flow
- JWT access tokens (RSA-2048 signed)
- Refresh tokens, ID tokens
- Discovery endpoint: `/.well-known/openid-configuration`
- JWKS: `/.well-known/jwks.json`
- Linked identities (Google, GitHub) — used for feature settings requiring external accounts
- User-scope assignment: Sven's user DB determines who has which feature scopes

**Frontend OIDC flow (when implemented):**
1. `OidcAuthProvider` uses `oidc-client-ts` `UserManager`
2. On page load, checks for existing session
3. If no session, redirects to Sven's authorize endpoint
4. Sven shows login UI, authenticates user, redirects back with auth code
5. `OidcAuthProvider` exchanges code for tokens (PKCE)
6. Access token JWT contains `scope` claim with feature scopes
7. `buildFeatureTree(scopes)` → `featureTree` → `useFeature()` / sidebar nav items

**Backend JWT validation:**
```csharp
builder.Services.AddBureauApiAuth(builder.Configuration);
```

In prod appsettings:
```json
{
  "Auth": {
    "Mode": "Oidc",
    "Oidc": {
      "Authority": "https://auth.bureau.home",
      "Audience": "bureau-api"
    }
  }
}
```

JWT bearer middleware validates the token against Sven's JWKS (fetched from discovery endpoint).
`[RequireFeature]` attribute then checks the `scope` claim in the validated token.

---

## 4. Feature scopes

Features are OAuth 2.0 scopes issued by Sven. They follow dot-path notation matching `FeatureKeys`:

| Scope | Feature |
|-------|---------|
| `niles.chores` | Chores admin |
| `watson.nodes` | Nodes admin (root) |
| `watson.nodes.crud` | Node create/update/delete |
| `watson.nodes.analytics` | Node analytics (not yet implemented) |

Parent scopes imply access to the feature group but not necessarily to child features.
A user with `watson.nodes` but not `watson.nodes.crud` can view nodes but cannot modify them.

The `isFeatureEnabled(tree, path)` function traverses the feature tree:
- `watson.nodes` in scopes → `watson.nodes.crud` returns false (not explicit)
- `watson.nodes.crud` in scopes → all of `watson.nodes.crud` returns true,
  `watson.nodes` also returns true (parent leaf is true)

---

## 5. Switching auth mode

**To test OIDC locally (when Sven is available):**

1. Edit `app/client/apps/bureau-web/public/config.json`:
   ```json
   {
     "auth": {
       "mode": "oidc",
       "oidc": {
         "authority": "http://localhost:5100",
         "clientId": "bureau-web",
         "redirectUri": "http://localhost:5000/callback"
       }
     }
   }
   ```

2. Set API appsettings to `Auth:Mode: Oidc` with Sven's authority and audience.

3. Make sure Sven is running on port 5100.

The app will redirect to Sven's login page. After login, feature scopes from the JWT
are used automatically — no code changes needed.
