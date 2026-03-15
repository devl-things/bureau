# Coding Conventions

**Analysis Date:** 2025-03-15

## Naming Patterns

**Files:**
- PascalCase for all C# files
- File name matches primary class name (e.g., `ClientProvider.cs` contains `public class ClientProvider`)
- Test files: `[Subject]Tests.cs` or `[Subject]UnitTests.cs` (e.g., `TokenProviderTests.cs`, `SignInHandlerUnitTests.cs`)
- Interfaces: `I[Name].cs` (e.g., `IClientProvider.cs`, `ISymEncryptor.cs`)

**Functions/Methods:**
- PascalCase for all public and internal methods
- Async methods: **mandatory `Async` suffix** (e.g., `CreateUserAsync`, `IsRefreshTokenValidAsync`, `GetClientAsync`)
- Test methods: descriptive names with format `[MethodName]_[Scenario]_[ExpectedResult]` (e.g., `IsRefreshTokenValidAsync_ValidToken_ReturnsTrue`, `StoreAsync_PersistsClient`)
- Private helper methods: PascalCase (e.g., `CreateNewClientId()`, `EncryptWithCombinedIV()`)

**Variables:**
- Local variables: camelCase (e.g., `clientId`, `refreshToken`, `userIdResult`)
- Constants: UPPER_SNAKE_CASE (e.g., `MAX_VERIFICATION_CODE`, `MIN_VERIFICATION_CODE`)
- Private fields: prefixed with underscore, camelCase (e.g., `_clientStore`, `_timeProvider`, `_key`)
- Constructor parameters: camelCase, no prefix (e.g., `clientId`, `redirectUri`)

**Types:**
- Classes: PascalCase (e.g., `ClientProvider`, `SvenTokenProvider`, `DiscoveryService`)
- Interfaces: PascalCase with `I` prefix (e.g., `IClientProvider`, `IUserStore`, `ISymEncryptor`)
- Enums: PascalCase (e.g., `VerificationStatus`, `ResponseTypes`)
- Namespaces: PascalCase, hierarchical by feature/layer (e.g., `Sven.Services`, `Bureau.AspNetCore.Middleware`, `Sven.Data`)

## Code Style

**Formatting:**
- EditorConfig enforced: `server/.editorconfig`
- Indentation: 4 spaces (never tabs)
- Line endings: CRLF (Windows)
- No final newline on files (`insert_final_newline = false`)
- Max line length: not enforced, but keep readable

**Linting:**
- EditorConfig configuration in `server/.editorconfig` enforces C# conventions
- Key rules:
  - `csharp_style_namespace_declarations = block_scoped:silent` — block-scoped namespaces only
  - `csharp_style_var_elsewhere = false` — **no `var`** keyword allowed
  - `csharp_style_expression_bodied_properties = true:silent` — properties may use expression bodies
  - `csharp_style_expression_bodied_methods = false` — methods must use explicit blocks
  - `dotnet_style_readonly_field = true` — prefer readonly fields
  - `dotnet_code_quality_unused_parameters = all` — no unused parameters

## Import Organization

**Order:**
1. System namespaces (`using System;`, `using System.Text;`)
2. External packages (`using Microsoft.*`, `using Sven.*;`)
3. Internal project namespaces (`using Bureau.*;`)

**Example:**
```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sven.Configurations;
using Sven.Data;
using Sven;
```

**Path Aliases:**
- No global using aliases detected
- Namespaces are fully qualified

**GlobalUsings:**
- Sven project uses GlobalUsings.cs: checks for `using Xunit;` in test project

## Error Handling

**Pattern: Result<T> / Result wrapper**
- All methods return `Result<T>` or `Result` instead of throwing exceptions
- `Result<T>` contains:
  - `IsSuccess` / `IsError` boolean flags
  - `Value` (for successful results)
  - `Error` (ResultError object with Code and ErrorMessage)
- Error construction: `ResultError.From(code)` or `ResultError.From(code, message)`

**Example from `ClientProvider.cs`:**
```csharp
public async Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken)
{
    if (!AuthConstants.OAuth.TokenAuthMethods.None.Equals(clientRequest.TokenEndpointAuthMethod))
    {
        return ResultError.From("Only supported client type is public", $"Received {nameof(clientRequest.TokenEndpointAuthMethod)} = {clientRequest.TokenEndpointAuthMethod};");
    }
    // ... more validation
    Result result = await _clientStore.StoreAsync(client, cancellationToken);
    if (result.IsError)
    {
        return result.Error;
    }
    return client;
}
```

**Error Propagation:**
- Check `IsError` immediately after async calls
- Return error up the chain: `if (result.IsError) return result.Error;`
- Chaining: `if (userIdResult.IsError) return userIdResult.Error;`

**Exception Handling:**
- Caught only at boundaries (crypto operations, serialization)
- Wrapped in Result: `catch (Exception ex) { return ResultError.From("encryption_error", ex); }`
- Middleware: `ApiExceptionHandlingMiddleware` catches unhandled exceptions and converts to ProblemDetails
- Location: `server/src/Bureau.AspNetCore/Middleware/ApiExceptionHandlingMiddleware.cs`

## Logging

**Framework:** Microsoft.Extensions.Logging (`ILogger<T>`)

**Patterns:**
- All services receive `ILogger<ServiceType>` via constructor injection
- Log level conventions:
  - `LogError()` / `LogResultError()` — for failures and errors
  - `LogInformation()` — for important lifecycle events
  - `LogDebug()` — for diagnostic information
- Custom extension: `BureauAspNetCoreLoggingExtensions.cs` provides `LogResultError()` helper
- Location: `server/src/Bureau.AspNetCore/Logging/BureauAspNetCoreLoggingExtensions.cs`

**Example:**
```csharp
private readonly ILogger<SvenTokenProvider> _logger;

public SvenTokenProvider(ILogger<SvenTokenProvider> logger, ...)
{
    _logger = logger;
}

// Log error
_logger.LogResultError(error);
_logger.LogResultError(error, httpContext);
```

## Comments

**When to Comment:**
- Document public API intent (what it does, not how)
- Reference issues/TODOs with `#<issue-number>` (e.g., `// #38 ClientSecret, ClientSecretExpiresAt`)
- Explain non-obvious security decisions or limitations
- DO NOT comment obvious code (e.g., loop logic, variable assignments)

**Example from `ClientProvider.cs`:**
```csharp
// #38 ClientSecret, ClientSecretExpiresAt
ClientUri = clientRequest.ClientUri,
```

**Example from `UserProvider.cs`:**
```csharp
// #53 make sure that using this pseudorandom number generator is safe here csharpsquid:S2245
string ticket = RandomNumberGenerator.GetInt32(MinVerificationCode, MaxVerificationCode).ToString();

// #53 add expiration date 5min
if (await _miscStore.StoreAsync(ticket, userIdentifier, cancellationToken) is { IsError: true } result)
```

**JSDoc/TSDoc:**
- No formal documentation generation (no `///` summaries required)
- Method summaries are present for validators: `/// <summary>Checks if...</summary>`
- Location: `server/src/Sven.Abstractions/Services/SvenValidators.cs`

## Function Design

**Size:**
- Keep functions focused on single responsibility
- Target: 20-50 lines for typical business logic
- Larger functions (200+ lines) are generators or seeding: `ChoresSeeder.cs` (960 lines) — acceptable for data generation

**Parameters:**
- Always include `CancellationToken cancellationToken = default` as last parameter for async methods (mandatory)
- Constructor injection preferred for dependencies (not parameter injection)
- Avoid boolean flags; use enum or separate methods

**Return Values:**
- Always return Result<T> (never throw)
- For boolean success, return `Result` (not Result<bool>)
- Example: `public async Task<Result> UpdatePasswordAsync(...)`
- Example: `public async Task<Result<Client>> GetClientAsync(...)`

## Module Design

**Exports:**
- All public classes/interfaces in a module are intended for external use
- Internal classes: marked `internal` (not public)
- Static helper classes: e.g., `SvenValidators` (static methods only)

**Barrel Files:**
- Not systematically used; imports are from specific namespaces
- Example: `using Sven.Services;` imports specific service, not a barrel

**Project Structure by Module:**
- Abstractions layer: `[Feature].Abstractions/` (interfaces and DTOs)
- Implementation layer: `[Feature]/` or `[Feature].Data/` or `[Feature].Data.[Provider]/`
- Example:
  - `server/src/Sven.Abstractions/` — interfaces (IClientProvider, IUserStore)
  - `server/src/Sven/Services/` — implementations (ClientProvider, UserProvider)
  - `server/src/Sven.Data.SqlServer/` — database implementations

## Async/Await Patterns

**Mandatory Rules:**
1. Every `async` method must have `Async` suffix in name
2. Every async method must accept `CancellationToken cancellationToken = default` as last parameter
3. Pass token to all async calls: `await _store.GetAsync(id, cancellationToken)`

**Example from `ClientProvider.cs`:**
```csharp
public async Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken)
{
    // ... validation
    Result result = await _clientStore.StoreAsync(client, cancellationToken);
    // ...
}
```

**Example from `UserProvider.cs`:**
```csharp
public async Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
{
    // ...
    if (await _userStore.StoreAsync(user, cancellationToken) is { IsError: true } storedResult)
    {
        return storedResult.Error;
    }
}
```

## Namespace Scope

**Rule: Block-scoped namespaces only**
- Never use file-scoped namespace syntax: `namespace Foo;`
- Always use explicit braces:

```csharp
namespace Sven.Services
{
    public class ClientProvider
    {
        // ...
    }
}
```

## Property Declaration

**Rule: Explicit get/set blocks only**
- Never use expression-body properties: `public string Name => _name;`
- Always use explicit blocks:

```csharp
public string Mode
{
    get { return _mode; }
}

public IReadOnlyList<ExternalScope> Scopes
{
    get { return _scopes; }
}
```

- Exception: Expression-bodied methods (single-expression methods) are acceptable where clearer

## No `var` Keyword

**Rule: Always declare explicit types**
- Never use `var` for type inference
- Always specify the full type:

```csharp
// CORRECT
List<ExternalScope> scopes = configuration.GetSection("Scopes").Get<List<ExternalScope>>() ?? [];
HttpResponseMessage response = await _client.PostAsync(url, content, cancellationToken);
Result<Client> client = await _clientStore.GetAsync(clientId, cancellationToken);

// WRONG — do not use var
var scopes = configuration.GetSection("Scopes").Get<List<ExternalScope>>() ?? [];
var response = await _client.PostAsync(url, content, cancellationToken);
```

---

*Convention analysis: 2025-03-15*
