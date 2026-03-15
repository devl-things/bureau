using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.Tests.Security
{
    [Trait("Category", "Phase1")]
    public class AuthCodeAtomicTests
    {
        private static AuthCodeService BuildAuthCodeService(ILogger<AuthCodeService>? logger = null)
        {
            IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions
            {
                AuthorizationCodeLifetime = TimeSpan.FromMinutes(5)
            });
            IStore<string, OAuthRequest> pkceStore = new InMemoryStore<string, OAuthRequest>();
            IStore<string, AuthCode> authCodeStore = new InMemoryStore<string, AuthCode>();
            ILogger<AuthCodeService> log = logger ?? Substitute.For<ILogger<AuthCodeService>>();
            return new AuthCodeService(authOptions, TimeProvider.System, pkceStore, authCodeStore, log);
        }

        private static AuthCode BuildAuthCode(string code = "test-code")
        {
            return new AuthCode
            {
                Code = code,
                ClientId = "client1",
                RedirectUri = "https://example.com/callback",
                Scope = "openid",
                Claims = new List<Claim>(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
            };
        }

        [Fact]
        public async Task ExchangeCode_ConcurrentSecondCall_ReturnsInvalidGrant()
        {
            // Two Tasks both call ExchangeCodeAsync with the same code.
            // Exactly one succeeds; the other must return invalid_grant.
            AuthCodeService service = BuildAuthCodeService();

            // Seed a code
            string code = "concurrent-test-code";
            AuthCode authCode = BuildAuthCode(code);
            IStore<string, AuthCode> authCodeStore = new InMemoryStore<string, AuthCode>();
            await authCodeStore.StoreAsync(code, authCode, CancellationToken.None);

            // Build service with the pre-seeded store
            IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions
            {
                AuthorizationCodeLifetime = TimeSpan.FromMinutes(5)
            });
            IStore<string, OAuthRequest> pkceStore = new InMemoryStore<string, OAuthRequest>();
            ILogger<AuthCodeService> logger = Substitute.For<ILogger<AuthCodeService>>();
            AuthCodeService serviceWithStore = new AuthCodeService(authOptions, TimeProvider.System, pkceStore, authCodeStore, logger);

            // Launch two concurrent calls
            Task<Result<AuthCode>> task1 = serviceWithStore.ExchangeCodeAsync(code, CancellationToken.None);
            Task<Result<AuthCode>> task2 = serviceWithStore.ExchangeCodeAsync(code, CancellationToken.None);
            Result<AuthCode>[] results = await Task.WhenAll(task1, task2);

            int successCount = results.Count(r => !r.IsError);
            int failCount = results.Count(r => r.IsError);

            Assert.Equal(1, successCount);
            Assert.Equal(1, failCount);
        }

        [Fact]
        public async Task ExchangeCode_ReuseAttempt_LogsSecurityWarning()
        {
            // Verifies that ILogger captures a Warning-level event when a code
            // reuse is detected.
            ILogger<AuthCodeService> logger = Substitute.For<ILogger<AuthCodeService>>();

            IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions
            {
                AuthorizationCodeLifetime = TimeSpan.FromMinutes(5)
            });
            IStore<string, AuthCode> authCodeStore = new InMemoryStore<string, AuthCode>();
            IStore<string, OAuthRequest> pkceStore = new InMemoryStore<string, OAuthRequest>();
            AuthCodeService service = new AuthCodeService(authOptions, TimeProvider.System, pkceStore, authCodeStore, logger);

            string code = "reuse-test-code";
            AuthCode authCode = BuildAuthCode(code);
            await authCodeStore.StoreAsync(code, authCode, CancellationToken.None);

            // First call: succeeds
            Result<AuthCode> firstResult = await service.ExchangeCodeAsync(code, CancellationToken.None);
            Assert.False(firstResult.IsError, "First call should succeed");

            // Second call: reuse attempt — should trigger a Warning log
            Result<AuthCode> secondResult = await service.ExchangeCodeAsync(code, CancellationToken.None);
            Assert.True(secondResult.IsError, "Second call (reuse) should return an error");

            logger.Received().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("reuse") || o.ToString()!.Contains("Security")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
