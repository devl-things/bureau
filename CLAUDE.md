# Bureau — Coding Conventions

These rules apply to all code in this repository. Follow them in every file touched, regardless of what surrounds the change.

## C# Formatting Rules

### 1. Explicit namespace scope — no file-scoped namespaces
Always use block-scoped namespaces with explicit braces. Never use the file-scoped `namespace Foo;` syntax.

```csharp
// ✅ correct
namespace Sven.Services
{
    public class MyService { }
}

// ❌ wrong
namespace Sven.Services;
public class MyService { }
```

### 2. Async methods — `Async` suffix + `CancellationToken` parameter
Every `async` method (and every method returning `Task`/`Task<T>`/`ValueTask`) must:
- Have the `Async` suffix in its name
- Accept a `CancellationToken cancellationToken = default` as the last parameter

```csharp
// ✅ correct
public async Task<Result<User>> GetUserAsync(string id, CancellationToken cancellationToken = default)

// ❌ wrong — missing suffix, missing token
public async Task<Result<User>> GetUser(string id)
```

### 3. No `var` — always use explicit types
Never use `var`. Always declare the explicit type. This applies everywhere: local variables, foreach loops, using declarations, out variables.

```csharp
// ✅ correct
List<ExternalScope> scopes = configuration.GetSection("Scopes").Get<List<ExternalScope>>() ?? [];
HttpResponseMessage response = await _client.PostAsync(url, content, cancellationToken);

// ❌ wrong
var scopes = configuration.GetSection("Scopes").Get<List<ExternalScope>>() ?? [];
var response = await _client.PostAsync(url, content, cancellationToken);
```

### 4. No expression-body property shorthand — use explicit `get`/`set`
Do not use `=>` to define property bodies or auto-derived properties. Use explicit `get`/`set` blocks.

```csharp
// ✅ correct
public string Mode
{
    get { return _mode; }
}

public IReadOnlyList<ExternalScope> Scopes
{
    get { return _scopes; }
}

// ❌ wrong
public string Mode => _mode;
public IReadOnlyList<ExternalScope> Scopes => _scopes;
```

This rule applies to properties. Short expression-bodied **methods** (single-expression methods) are acceptable where they are genuinely clearer.
