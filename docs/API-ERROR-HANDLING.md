
# API Error Handling & Response Conventions

This document defines the standard response format and error-handling conventions for all HTTP APIs in the **bureau** monorepo (e.g. Niles/chores, Sven, and future services).

The goals are:

- Predictable, boring responses.
- Clear separation between success and failure.
- Reuse of ASP.NET Core and BCL primitives (e.g. `ProblemDetails`).
- Easy correlation between UI-visible errors and backend logs.

---

## 1. Success Responses

### 1.1. Single Resource

For a successful operation that returns a single resource, the response MUST use HTTP 2xx (usually `200 OK` or `201 Created`) and a JSON body of the form:

```jsonc
{
  "data": {
    // resource representation
  }
}
```

Examples:

- `GET /api/housekeeping/{id}` → `200 OK`
- `POST /api/housekeeping` → `201 Created`

The `data` field contains the resource (or DTO) in its canonical JSON representation.

### 1.2. Collections and Pagination

For list responses, the body MUST include `data` as an array and MAY include a `meta` object for pagination or other metadata:

```jsonc
{
  "data": [
    { /* item 1 */ },
    { /* item 2 */ }
  ],
  "meta": {
    "total": 42,
    "page": 1,
    "pageSize": 20,
    "hasNextPage": true
  }
}
```

The exact shape of `meta` can vary per endpoint but should remain consistent within each API.

### 1.3. Commands / Mutations

- `POST` creating a new resource SHOULD respond with `201 Created` and the created resource in `data`.
- `PUT`/`PATCH` updating a resource SHOULD respond with `200 OK` (preferred) or `204 No Content` when no additional data is useful.
- `DELETE` SHOULD respond with `204 No Content` when successful.

Examples:

```jsonc
// 201 Created
{
  "data": {
    "id": "hk_987",
    "dateTime": "2025-12-17T09:00:00Z",
    "duration": "00:45:00",
    "note": "Quick clean-up"
  }
}
```

---

## 2. Error Responses

All non-2xx responses MUST use [`ProblemDetails`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.problemdetails) as the response body type.

`ProblemDetails` is defined by RFC 7807 and implemented in `Microsoft.AspNetCore.Mvc`. It has these core fields:

- `type`: A URI identifying the problem type.
- `title`: A short, human-readable summary of the problem type.
- `status`: The HTTP status code.
- `detail`: A human-readable explanation of the specific problem.
- `instance`: A URI reference that identifies the specific occurrence of the problem (usually the request path).

We extend `ProblemDetails` using its `Extensions` dictionary to include additional fields:

- `code`: Machine-readable error code (`string`).
- `errors`: Per-field validation errors (for 4xx validation failures).
- `traceId`: Request correlation identifier (for logs and support).

### 2.1. JSON Shape

`ProblemDetails.Extensions` entries are serialized as top-level properties in the JSON representation. The final error payload looks like this:

```jsonc
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/housekeeping",
  "code": "validation.failed",
  "errors": {
    "date": ["Invalid date format. Use yyyy-MM-dd."],
    "duration": ["Duration is required."]
  },
  "traceId": "00-5f37e8d29759c14a4e9f8b7ff33f3c4c-3b5c8f64558fb94b-00"
}
```

There is **no additional `error` wrapper** around `ProblemDetails`. Clients distinguish success vs error by:

- HTTP status code (2xx vs non-2xx), and
- The presence of `ProblemDetails` fields (`type`, `title`, `status`, etc.).

### 2.2. Error Codes

The `code` field is a **stable, machine-readable error identifier**. It SHOULD follow a namespaced convention:

- General structure: `area.reason`.
- Examples:
  - `validation.failed`
  - `validation.invalid_date`
  - `housekeeping.not_found`
  - `housekeeping.overlapping_entry`
  - `auth.unauthorized`
  - `system.unexpected_error`

Clients MAY use `code` for conditional behavior (e.g. showing specific UI messages), but SHOULD NOT rely on `detail` or `title` text.

### 2.3. Validation Errors (`errors`)

For validation failures (`400 Bad Request`), the `errors` extension SHOULD be used to provide per-field messages:

```jsonc
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/housekeeping",
  "code": "validation.failed",
  "errors": {
    "date": ["Invalid date format. Use yyyy-MM-dd."],
    "duration": ["Duration is required.", "Duration must be greater than zero."]
  },
  "traceId": "00-..."
}
```

This enables frontends to map messages directly onto form fields without parsing free-form text.

### 2.4. `traceId` and Correlation

Each request has a correlation ID (`traceId`) used to tie logs and error responses together. It is populated from:

```csharp
string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
```

The same `traceId` MUST be:

- Logged with each error (at minimum).
- Exposed in the `ProblemDetails` payload via the `traceId` extension.

Frontends MAY show this identifier to users when errors occur, for example:

> “If the issue persists, contact support and provide code: `<traceId>`.”

---

## 3. Result Pattern in the Domain/Service Layer (TBD)

The project already uses a `ResultError` struct and related result types for representing
success/failure outcomes in the domain/service layer. The exact conventions (naming,
namespaces, and usage across services) will be documented here later, once they stabilize.

## 4. Error → ProblemDetails Mapping (TBD)

The precise mapping between the existing result-error structures (e.g. `ResultError`) and
`ProblemDetails` will be defined here after the initial implementation is in place in Niles
and has proven itself in practice.



A helper is provided to convert an `Error` into `ProblemDetails` in a consistent way.

### 4.1. Extension Method

In `Bureau.AspNetCore`:

```csharp
public static ProblemDetails ToProblemDetails(this ResultError error, HttpContext httpContext, int statusCode)
```

Controllers use this helper to translate failed `ResultError` values into HTTP responses.

---

## 5. Global Exception Handling Middleware

A shared middleware handles unexpected exceptions and translates them into `ProblemDetails` responses with `500 Internal Server Error`.

The middleware lives in the shared **`Bureau.AspNetCore`** project and is reused by all web/API applications.

### 5.1. Responsibilities

- Catch all unhandled exceptions.
- Log them with the current `traceId`.
- Return a `ProblemDetails` payload with:
  - `status` = `500`
  - `code` = `system.unexpected_error`
  - Generic `detail` message
  - `traceId`

Example error payload:

```jsonc
{
  "type": "https://httpstatuses.com/500",
  "title": "An unexpected error occurred.",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "/api/housekeeping",
  "code": "system.unexpected_error",
  "traceId": "00-..."
}
```

### 5.2. Registration

In each ASP.NET Core app (e.g. Niles API):

```csharp
app.UseMiddleware<ApiExceptionHandlingMiddleware>();
```

This middleware should be registered early in the pipeline, after routing but before endpoints, so it can see exceptions from controllers and other middleware.

---

## 6. Logging and Trace Correlation

To ensure all logs for a given request share the same `traceId`, ASP.NET Core’s built-in activity tracking is used.

### 6.1. Activity Tracking

ASP.NET Core uses `Activity` (from `System.Diagnostics`) to track each HTTP request. To ensure `Activity.Current` is populated and trace information flows into logs, activity tracking MUST be enabled in `Program.cs`:

Enable activity tracking from 
```csharp
public static ILoggingBuilder AddBureauActivityTracking(this ILoggingBuilder logging)
```
in `Program.cs`:

```csharp
builder.Logging.AddBureauActivityTracking();
```

This ensures:

- `Activity.Current` is set for each HTTP request.
- `TraceId` is automatically attached to logs by compatible providers.



If necessary, a logging scope can explicitly add the `traceId` to log state, but this is optional when using activity tracking. For example, in `ApiExceptionHandlingMiddleware` you may do:

```csharp
string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

using IDisposable scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["TraceId"] = traceId
});
```

This is only needed if you want a specific property name or additional custom context beyond what activity tracking provides.

---

## 8. Client Expectations



Clients (web apps, mobile apps, integrations) should assume:

1. **On success (2xx)**:
   - Response contains `data` (and optionally `meta`).
   - Shape of `data` is specific to the endpoint.

2. **On error (non-2xx)**:
   - Response is a `ProblemDetails` payload with possible extensions:
     - `code` (machine-readable)
     - `errors` (for validation)
     - `traceId` (for troubleshooting)
   - Clients MUST NOT rely on `detail` text for behavior, only for display.

This contract should be considered part of the public API surface for all bureau services.
