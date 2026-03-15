# Testing Patterns

**Analysis Date:** 2025-03-15

## Test Framework

**Runner:**
- xUnit 2.5.3
- Config: `server/tests/Sven.Tests/Sven.Tests.csproj`

**Assertion Library:**
- xUnit built-in assertions: `Assert.True()`, `Assert.Equal()`, `Assert.Contains()`, `Assert.Empty()`, etc.

**Mocking/Substitution:**
- NSubstitute 5.3.0
- Integration testing: Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)

**Run Commands:**
```bash
dotnet test                          # Run all tests in solution
dotnet test server/tests/Sven.Tests  # Run Sven test project
dotnet test --filter "Category=Unit" # Run only unit tests
dotnet test --collect:"XPlat Code Coverage"  # With coverage
```

**Coverage:**
- Tool: coverlet.collector 6.0.0
- Generate coverage: `dotnet test --collect:"XPlat Code Coverage"`
- View coverage report in `TestResults/` directory
- No explicit coverage target enforced

## Test File Organization

**Location:**
- Test files co-located with Sven project under `server/tests/Sven.Tests/`
- Structure mirrors source organization:
  - `server/tests/Sven.Tests/Services/` → tests for `server/src/Sven/Services/`
  - `server/tests/Sven.Tests/Controllers/` → tests for `server/src/Sven.Web/Controllers/`
  - `server/tests/Sven.Tests/Stores/` → tests for store implementations
  - `server/tests/Sven.Tests/Pages/Connect/SignUp/` → tests for page handlers
  - `server/tests/Sven.Tests/Fixtures/` → test fixtures and factories

**Naming:**
- `[SubjectClass]Tests.cs` or `[Subject][UnitType]Tests.cs`
- Examples:
  - `TokenProviderTests.cs`
  - `ClientStoreTests.cs`
  - `SignInHandlerUnitTests.cs`
  - `SignUpStepHandlerUnitTests.cs`
  - `ConnectControllerTests.cs`
  - `ExternalProviderRegistryTests.cs`

**Directory Structure:**
```
server/tests/Sven.Tests/
├── Controllers/
│   ├── ConnectControllerTests.cs
│   ├── ConnectControllerErrorTests.cs
│   ├── ConnectControllerRevocationTests.cs
│   └── ConnectControllerTokenTests.cs
├── Services/
│   ├── TokenProviderTests.cs
│   ├── ExternalProviderRegistryTests.cs
│   ├── ExternalProviderScopeConfigTests.cs
│   └── PasswordHasherTests.cs
├── Stores/
│   ├── ClientStoreTests.cs
│   └── SvenUserStoreTests.cs
├── Pages/
│   └── Connect/
│       ├── SignIn/
│       │   ├── PkceSignInTests.cs
│       │   └── SignInHandlerUnitTests.cs
│       └── SignUp/
│           ├── ForgotSignUpTests.cs
│           ├── PlainSignUpTests.cs
│           └── SignUpStepHandlerUnitTests.cs
├── Fixtures/
│   └── SvenWebAppFactory.cs
└── TestData/
    └── (test data and constants)
```

## Test Structure

**Suite Organization:**
```csharp
using Xunit;  // Provided globally in Sven.Tests.csproj

namespace Sven.Tests.Services
{
    public class TokenProviderTests
    {
        // Fields for dependencies
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly IRefreshTokenService _refreshTokenStore;
        private readonly TimeProvider _timeProvider;
        private readonly IClientProvider _clientProvider;
        private readonly SvenTokenProvider _provider;

        // Test data fields
        private readonly string _clientId = "client1";
        private readonly string _redirectUri = "http://localhost";

        // Constructor: setup (xUnit calls before each test)
        public TokenProviderTests()
        {
            _logger = Substitute.For<ILogger<SvenTokenProvider>>();
            _refreshTokenStore = Substitute.For<IRefreshTokenService>();
            _timeProvider = Substitute.For<TimeProvider>();
            _clientProvider = Substitute.For<IClientProvider>();
            _provider = new SvenTokenProvider(
                _logger,
                Options.Create(new JwtOptions()),
                new RsaSecurityKey(RSA.Create(2048)),
                _refreshTokenStore,
                _timeProvider,
                _clientProvider
            );
        }

        // Individual test
        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            RefreshToken validToken = new RefreshToken() { /* ... */ };
            _refreshTokenStore.GetAsync("valid_token", Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(validToken));

            // Act
            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "valid_token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
```

**Patterns:**
- **Constructor setup:** NSubstitute mocks created in constructor, reused for all tests
- **Arrange-Act-Assert:** Tests follow AAA pattern (inline comments optional but shown above)
- **Teardown:** xUnit automatically disposes `IAsyncLifetime` implementations; explicit Dispose() for integration tests
- **Traits:** Every test has `[Trait("Category", "Unit")]` or `[Trait("Category", "Integration")]`

## Mocking

**Framework:** NSubstitute 5.3.0

**Patterns:**
```csharp
// Create substitute
IClientStore store = Substitute.For<IClientStore>();

// Setup return value
store.GetAsync("id", Arg.Any<CancellationToken>())
    .Returns(new Result<Client>(expectedClient));

// Setup to return error
store.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(new Result<Client>(resultError));

// Verify calls made
store.Received(1).GetAsync("valid_token", Arg.Any<CancellationToken>());
_logger.Received(1).LogResultError(resultErrorExpected);

// TimeProvider mock (static method stubs)
TimeProvider timeProvider = Substitute.For<TimeProvider>();
timeProvider.GetUtcNow().Returns(DateTime.UtcNow);
```

**What to Mock:**
- External services/repositories: `IClientStore`, `IRefreshTokenService`, `IUserStore`
- Infrastructure: `TimeProvider`, `ILogger<T>`, configuration `IOptions<T>`
- External APIs: `IHttpClientFactory`, HTTP clients

**What NOT to Mock:**
- Domain models: `Client`, `RefreshToken`, `SvenUser` — instantiate directly
- Value objects: `Result<T>`, `ResultError`, `ScopeParameter` — instantiate directly
- Static helpers: use actual implementations if testing Result-type patterns

## Fixtures and Factories

**Test Data:**
```csharp
// From SvenWebAppFactory.cs
private static async Task SeedTestDataAsync(IServiceProvider services)
{
    IUserStore userStore = services.GetRequiredService<IUserStore>();
    IClientStore clientStore = services.GetRequiredService<IClientStore>();

    await userStore.StoreAsync(new SvenUser
    {
        DisplayName = TestDataConstants.TestUserUsername,
        Username = TestDataConstants.TestUserUsername,
        PasswordHash = PasswordHasher.HashPassword(TestDataConstants.TestUserPassword)
    }, CancellationToken.None);

    await clientStore.StoreAsync(new Client
    {
        Identifier = TestDataConstants.TestClientId,
        Name = "Test Client",
        CreatedAt = DateTimeOffset.UtcNow,
        // ... more fields
    }, CancellationToken.None);
}
```

**WebApplicationFactory Pattern:**
- `SvenWebAppFactory : WebApplicationFactory<Program>`
- Overrides `ConfigureWebHost()` to:
  - Set environment to "Testing"
  - Add in-memory database (EntityFrameworkCore.InMemory)
  - Clear providers and add test logger provider
  - Seed test data in `CreateHost()` override
- Exposes `LoggerProvider` property for assertion on logs
- Location: `server/tests/Sven.Tests/Fixtures/SvenWebAppFactory.cs`

**Test Data Constants:**
- Centralized in `TestDataConstants` class
- Examples: `TestClientId`, `TestUserUsername`, `TestClientRedirectUri`, `ExistingUserEmail`
- Location: `server/tests/Sven.Tests/TestData/`

## Coverage

**Requirements:** None explicitly enforced (no CI gate on coverage %)

**View Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
# Coverage report generated in TestResults/
```

**Coverage Status:**
- Sven feature (OAuth2 OIDC) has substantial unit and integration test coverage
- Service layer: high coverage (TokenProvider, ClientProvider, UserProvider)
- Store/repository layer: integration tests using WebApplicationFactory
- Controller/endpoint layer: integration tests via WebApplicationFactory
- Data seeding: seed methods tested implicitly through integration tests

## Test Types

**Unit Tests:**
- Scope: Single service/class in isolation
- Mocks: All dependencies mocked via NSubstitute
- Examples:
  - `TokenProviderTests.IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()`
  - `ExternalProviderRegistryTests.GetScopes_KnownProvider_ReturnsDeclaredScopes()`
- Trait: `[Trait("Category", "Unit")]`
- Speed: Milliseconds, no I/O

**Integration Tests:**
- Scope: Full application stack or cross-service interactions
- Uses: `WebApplicationFactory<Program>` with in-memory database
- Examples:
  - `ConnectControllerTests.FullOAuthFlow_ReturnsAllTokens()` — Full OAuth2 flow with HTTP requests
  - `ClientStoreTests.StoreAsync_PersistsClient()` — Database persistence
  - `SvenUserStoreTests.*` — User store CRUD operations
- Trait: `[Trait("Category", "Integration")]`
- Speed: Seconds (slower due to application startup)
- Setup: Tests implement `IClassFixture<WebApplicationFactory<Program>>` or `IClassFixture<SvenWebAppFactory>`

**E2E Tests:**
- Framework: None detected (Docker Compose for deployment testing only)
- Not included in xUnit test suite

## Common Patterns

**Async Testing:**
```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()
{
    RefreshToken validToken = new RefreshToken() { /* ... */ };
    _refreshTokenStore.GetAsync("valid_token", Arg.Any<CancellationToken>())
        .Returns(new Result<RefreshToken>(validToken));

    Result<bool> result = await _provider.IsRefreshTokenValidAsync(
        "valid_token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

    Assert.True(result.IsSuccess);
}
```

**Error Testing (Result<T> pattern):**
```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task IsRefreshTokenValidAsync_TokenNotFound_ReturnsInvalidGrant()
{
    ResultError resultErrorExpected = ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound);
    _refreshTokenStore.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
        .Returns(new Result<RefreshToken>(resultErrorExpected));

    Result<bool> result = await _provider.IsRefreshTokenValidAsync(
        "missing_token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.Code);
    Assert.Equal(AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound, result.Error.ErrorMessage);
    _logger.Received(1).LogResultError(resultErrorExpected);
}
```

**Integration Test Pattern (Full Flow):**
```csharp
public class ConnectControllerTests : IClassFixture<SvenWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;

    public ConnectControllerTests(SvenWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = true
        });
    }

    [Fact(DisplayName = "oauth with pkce gets access, refresh and id token")]
    [Trait("Category", "Integration")]
    [Trait("Type", "Happy path")]
    public async Task FullOAuthFlow_ReturnsAllTokens()
    {
        // Step 1: Call /connect/authorize
        HttpResponseMessage authorizeResponse = await _client.GetAsync(authorizeUrl);
        Assert.Equal(HttpStatusCode.OK, authorizeResponse.StatusCode);

        // Step 2: Login with credentials
        HttpResponseMessage loginResponse = await LoginUserAsync(authorizeResponseContent, pkceKey);
        // Extract auth code from redirect

        // Step 3: Exchange code for tokens
        HttpResponseMessage tokenResponse = await _client.SendAsync(tokenRequest);
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        SvenToken token = JsonSerializer.Deserialize<SvenToken>(await tokenResponse.Content.ReadAsStringAsync())!;

        // Validate token claims
        string? nonce = GetClaimFromIdToken(token.IdToken!, JwtRegisteredClaimNames.Nonce);
        Assert.Equal(expectedNonce, nonce);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _factory.Dispose();
    }
}
```

**Database Integration Test Pattern:**
```csharp
public class ClientStoreTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ClientStoreTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Type", "Happy path")]
    public async Task StoreAsync_PersistsClient()
    {
        Client client = new Client { /* ... */ };

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IClientStore store = scope.ServiceProvider.GetRequiredService<IClientStore>();

            Result result = await store.StoreAsync(client, CancellationToken.None);
            Assert.True(result.IsSuccess);

            Result<Client> savedClientResult = await store.GetAsync(client.Identifier, CancellationToken.None);
            Assert.True(savedClientResult.IsSuccess);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _factory.Dispose();
    }
}
```

## Test Traits

**Standard Traits:**
- `[Trait("Category", "Unit")]` — unit test
- `[Trait("Category", "Integration")]` — integration test
- `[Trait("Type", "Happy path")]` — success case
- `[Trait("Type", "Error case")]` — failure/error scenario

**Usage:**
```bash
dotnet test --filter "Category=Unit"         # Run only unit tests
dotnet test --filter "Category=Integration"  # Run only integration tests
dotnet test --filter "Type=\"Happy path\""   # Run only happy path tests
```

## Test Organization by Feature

**Sven (OAuth2/OIDC Feature):**
- Controllers: `ConnectControllerTests.cs` + error variants (`ConnectControllerErrorTests.cs`, `ConnectControllerRevocationTests.cs`, `ConnectControllerTokenTests.cs`)
- Services: `TokenProviderTests.cs`, `ExternalProviderRegistryTests.cs`, `PasswordHasherTests.cs`
- Stores: `ClientStoreTests.cs`, `SvenUserStoreTests.cs`
- Page handlers: `SignInHandlerUnitTests.cs`, `SignUpStepHandlerUnitTests.cs`
- Pages (HTTP): `ConnectControllerTests.cs`, `PkceSignInTests.cs`, `ForgotSignUpTests.cs`, `PlainSignUpTests.cs`

**Coverage Gaps:**
- UI layer (`Pages/*.cshtml`) — minimal direct testing
- Email notification service — no direct tests (called via integration tests)
- File signature validators (Watson.Items.Ingest) — may have gaps
- Node attribute conventions (Watson.Nodes) — may have gaps

---

*Testing analysis: 2025-03-15*
