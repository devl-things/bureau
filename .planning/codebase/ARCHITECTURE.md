# Architecture

## Pattern
**Multi-service backend + single-tenant SPA** — each domain is an independent stateless .NET API service with its own database, fronted by a React SPA that lazy-loads domain feature modules. Auth is handled by Sven, a self-hosted OIDC provider.

## Architectural Style
- Backend: **Clean Architecture** with layers: Abstractions → Domain Logic → Data → API
- Error handling: **Railway-oriented** via `Result<T>` / `Result` structs (no exceptions for business logic)
- Auth: **JWT bearer tokens** with feature scopes in claims; validated server-side via `[RequireFeature]` attribute
- Feature access: **OAuth 2.0 scopes** in JWT claims (not config flags)

## Backend Services

| Service | Port | Responsibility |
|---|---|---|
| Sven.Web | (OIDC host) | Self-hosted OIDC / OAuth 2.0 identity provider (Razor Pages) |
| Niles.Chores.Api | 5300 | Household chores and housekeeping management |
| Watson.Nodes.Api | 5200 | Generic node graph (items, projects, tags, variants, edges) |
| Watson.Items.Ingest.Api | 5210 | ETL pipeline for ingest / background jobs |

## Layers (per domain)

```
[Domain].Api           — HTTP Controllers, DTOs, mappers, factories, Program.cs
[Domain]               — Business logic, services, EF data access, EF migrations
[Domain].Abstractions  — Interfaces, models, commands, queries (no implementations)
[Domain].Contracts     — API route constants, shared DTO definitions
```

Shared cross-cutting libraries:
- `Bureau.Primitives` — `Result<T>`, `ResultError`, `ProblemCodes`, `FeatureKeys`, paging models
- `Bureau.AspNetCore` — base controller, CORS, auth extensions, middleware, problem details, logging
- `Bureau.EntityFrameworkCore` — shared EF type conversions
- `Bureau.Server.Hosting` — JWT auth setup, dev auth handlers (`DevApiTokenAuthenticationHandler`)
- `Bureau.Server.Contracts` — shared API route patterns, cross-service mappers

## Data Flow (request lifecycle)

```
HTTP Request
  → ApiExceptionHandlingMiddleware (catch unhandled exceptions → ProblemDetails)
  → Authentication (JWT bearer or Dev handler)
  → Authorization ([RequireFeature] attribute checks JWT claim)
  → Controller (validates ModelState → returns ProblemDetails on invalid)
  → Service (business logic → returns Result<T>)
  → Repository/EF Context (database)
  → Controller maps Result<T> → IActionResult (Ok / ProblemDetailsResponse)
```

## Key Abstractions

### Result<T> — railway error handling
```csharp
// Bureau.Primitives/Models/Result.cs
Result<Node> result = await _nodeService.CreateAsync(command, cancellationToken);
if (result.IsError)
    return ProblemDetailsResponse(result.Error);
return CreatedAtRoute(..., result.Value.ToDto());
```

### BureauApiControllerBase
All controllers inherit from `BureauApiControllerBase` (`Bureau.AspNetCore/Controllers/`) which provides:
- `ProblemDetailsResponse(ResultError)` — maps domain errors to RFC 7807 problem details
- `ProblemDetailsResponse(ModelState)` — validation error response

### INodeKindHandler — Strategy pattern
Watson.Nodes uses a strategy pattern for node kinds:
- `INodeKindHandler` (abstraction) implemented by `DefaultNodeKindHandler`, `ItemNodeKindHandler`, `ProjectNodeKindHandler`, `TagNodeKindHandler`, `VariantNodeKindHandler`

### [RequireFeature] — Feature-gated access
```csharp
// Bureau.AspNetCore/Features/RequireFeatureAttribute.cs
[RequireFeature(FeatureKeys.Nodes)]
public class NodesController : NodesControllerBase { }
```

## Frontend Architecture

```
app/client/
  apps/
    bureau-web/         — Main SPA (React + Vite, port 5000)
    niles-chores-web/   — Standalone chores SPA (legacy / standalone)
  libs/
    bureau-shell/       — Shell: Layout, Header, Sidebar, theme, auth context
    client-core/        — Shared utilities, API clients, types
    chores-admin/       — Chores feature module (lazy loaded by bureau-web)
    nodes-admin/        — Nodes feature module (lazy loaded by bureau-web)
```

Feature modules (`chores-admin`, `nodes-admin`) export root components consumed via `window.__BUREAU__` bridge pattern by `bureau-web`.

## Auth Flow (Sven OIDC)
1. User hits bureau-web → redirects to Sven.Web (OIDC authorization endpoint)
2. Sven.Web handles sign-in (Razor Pages) with optional external providers (Google, Microsoft)
3. Sven issues JWT with feature scopes in claims
4. bureau-web stores token → passes as Bearer on API calls
5. API validates JWT → checks `[RequireFeature]` scope claim

## Dev Mode
`Bureau.Server.Hosting` provides `DevApiTokenAuthenticationHandler` and `DevPrincipalFactory` — injects a dev principal with configured scopes so APIs work without a real Sven instance during development.
