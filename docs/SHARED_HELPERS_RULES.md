# Shared helpers placement rules

This document defines where shared helper code should live in the `bureau` monorepo.

The goal is to avoid duplication while preserving **clear boundaries**, **clean dependency direction**, and **low coupling**.

---

## 1. First principles

### 1.1 Don’t turn domains into shared libraries
Domain projects (e.g., `Sven`, `Watson`, `Niles`, `Moneypenny`) should not become dumping grounds for reusable helpers.

If a helper is broadly reusable, move it into a **Bureau-level** shared project.

### 1.2 Primitives stay dependency-light
`Bureau.Primitives` is the lowest-level package. It should only contain contracts and value types that have minimal dependencies.

### 1.3 Technology-specific helpers belong in technology-named packages
If a helper requires a specific technology/framework (EF Core, ASP.NET Core, Logging), it must live in a correspondingly named project.

---

## 2. Canonical shared projects

### 2.1 `Bureau.Primitives`
**Use for:** shared meaning / contracts

**Examples**
- Marker interfaces (e.g., `IAuditable`)
- Result/error primitives
- IDs/value objects
- Stable enums used across domains

**Rules**
- ✅ `System.*` dependencies only
- ❌ no EF Core
- ❌ no ASP.NET Core
- ❌ no logging/DI

---

### 2.2 `Bureau.EntityFrameworkCore`
**Use for:** provider-neutral EF Core conventions and reusable mappings

**Examples**
- `AuditTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>`
- `ModelBuilder` / `EntityTypeBuilder` extensions (provider-neutral)

**Rules**
- ✅ may depend on `Microsoft.EntityFrameworkCore`
- ✅ may depend on `Bureau.Primitives`
- ❌ no DbContexts/migrations
- ❌ no domain entities

---

### 2.3 `Bureau.EntityFrameworkCore.SqlServer` (optional)
**Use for:** SQL Server–specific EF Core behavior

**Examples**
- `.HasColumnType("datetimeoffset")`
- provider-specific conventions

**Rules**
- ✅ depends on `Bureau.EntityFrameworkCore`
- ✅ may depend on `Microsoft.EntityFrameworkCore.SqlServer`
- ❌ no domain logic

---

### 2.4 `Bureau.Extensions.Logging`
**Use for:** `ILogger` helpers and logging conventions

**Examples**
- `ILogger` extension methods
- shared structured logging helpers
- common logging scope helpers

**Rules**
- ✅ may depend on `Microsoft.Extensions.Logging.*`
- ✅ may depend on `Bureau.Primitives`
- ❌ no ASP.NET Core
- ❌ no EF Core

---

### 2.5 `Bureau.Extensions.Time`
**Use for:** time/date helpers and `TimeProvider` extensions

**Examples**
- `TimeProvider` extension methods
- date/time helper functions

**Rules**
- ✅ `System.*` only (preferred)
- ✅ may depend on `Bureau.Primitives`
- ❌ no ASP.NET Core
- ❌ no EF Core

---

### 2.6 `Bureau.AspNetCore`
**Use for:** ASP.NET Core presentation-layer helpers

**Examples**
- `HttpRequest`, `HttpResponse`, `HttpContext` extension methods
- small ASP.NET Core utilities

**Rules**
- ✅ may depend on `Microsoft.AspNetCore.Http.*`
- ✅ may depend on `Microsoft.Extensions.Primitives`
- ❌ no domain logic
- ❌ no controllers/middleware pipelines

Note: ASP.NET Core–specific helpers that implement or extend framework types (for example, an `IHealthCheck` implementation like `ProbeHealthCheck<TProbe>`) belong in `Bureau.AspNetCore`. The probe contract itself (`IHealthProbe`) should be a minimal, framework-agnostic contract placed in `Bureau.Primitives`.

---

## 3. Decision tree

Use this checklist to place a helper:

1. **Does it define shared meaning or a contract?**
   - ✅ Yes → `Bureau.Primitives`

2. **Does it extend EF Core types or implement EF conventions?**
   - ✅ Yes → `Bureau.EntityFrameworkCore`
   - ✅ And if provider-specific → `Bureau.EntityFrameworkCore.SqlServer`

3. **Does it extend ASP.NET Core types (`HttpRequest`, `HttpContext`, etc.)?**
    - ✅ Yes → `Bureau.AspNetCore`

4. **Is it purely logging-related (`ILogger`)?**
   - ✅ Yes → `Bureau.Extensions.Logging`

5. **Is it time-related (`TimeProvider`, date/time helpers)?**
    - ✅ Yes → `Bureau.Extensions.Time`

6. **Is it domain-specific?**
   - ✅ Yes → keep it in the domain project (don’t share it)

---

## 4. What should NOT be shared

Do not centralize:
- domain-specific business rules
- user/context-specific behavior (e.g., how `CreatedBy` is determined)
- application workflows that belong to a single domain

---

## 5. Naming conventions

- Prefer technology names that match upstream ecosystems:
  - `*.EntityFrameworkCore`, `*.AspNetCore`, `*.Extensions.Logging`

- Extension classes should follow BCL conventions:
  - `HttpRequestExtensions` (plural)
  - Avoid “Utility/Helper” names where possible

---

## 6. Example mappings

- `IAuditable` → `Bureau.Primitives`
- `AuditTypeConfiguration<TEntity>` → `Bureau.EntityFrameworkCore`
- SQL Server `datetimeoffset` column type → `Bureau.EntityFrameworkCore.SqlServer`
- `HttpRequest` query/form/cookie helpers → `Bureau.AspNetCore`
- `ILogger` convenience methods → `Bureau.Extensions.Logging`
- `TimeProvider` helpers → `Bureau.Extensions.Time`

