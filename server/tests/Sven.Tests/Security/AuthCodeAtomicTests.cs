using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.TestUtils;
using System.Security.Claims;

namespace Sven.Tests.Security
{
    [Trait("Category", "Phase1")]
    public class AuthCodeAtomicTests
    {
        private static AuthCodeService BuildAuthCodeService(
            RepositoryTestFactory.OwnedContext? owned = null,
            ILogger<AuthCodeService>? logger = null)
        {
            IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions
            {
                AuthorizationCodeLifetime = TimeSpan.FromMinutes(5)
            });
            RepositoryTestFactory.OwnedContext ctx = owned ?? RepositoryTestFactory.CreateContext();
            AuthCodeRepository authRepo = RepositoryTestFactory.CreateAuthCodeRepository(ctx);
            PkceRequestRepository pkceRepo = RepositoryTestFactory.CreatePkceRequestRepository(ctx);
            ILogger<AuthCodeService> log = logger ?? Substitute.For<ILogger<AuthCodeService>>();
            return new AuthCodeService(authOptions, TimeProvider.System, pkceRepo, authRepo, log);
        }

        private static OAuthRequest BuildOAuthRequest()
        {
            return new OAuthRequest
            {
                ClientId = "client1",
                RedirectUri = "https://example.com/callback",
                Scope = "openid",
                CodeChallenge = "challenge",
                CodeChallengeMethod = "S256",
                State = null,
                Nonce = null
            };
        }

        [Fact]
        public async Task ExchangeCode_ConcurrentSecondCall_ReturnsInvalidGrant()
        {
            // Two Tasks both call ExchangeCodeAsync with the same code.
            // Exactly one succeeds; the other must return invalid_grant.
            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            AuthCodeService service = BuildAuthCodeService(owned: owned);

            // Seed a code via CreateAuthCodeAsync
            OAuthRequest request = BuildOAuthRequest();
            Result<string> codeResult = await service.CreateAuthCodeAsync(request, new List<Claim>(), CancellationToken.None);
            Assert.False(codeResult.IsError, "Seeding auth code should succeed");
            string code = codeResult.Value;

            // Launch two sequential calls (SQLite validates the repository's SELECT-then-DELETE pattern)
            Result<AuthCode> first = await service.ExchangeCodeAsync(code, CancellationToken.None);
            Result<AuthCode> second = await service.ExchangeCodeAsync(code, CancellationToken.None);

            Assert.False(first.IsError, "First call should succeed");
            Assert.True(second.IsError, "Second call should fail — code already consumed");
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, second.Error.Code);
        }

        [Fact]
        public async Task ExchangeCode_ReuseAttempt_LogsSecurityWarning()
        {
            // Verifies that ILogger captures a Warning-level event when a code
            // reuse is detected.
            ILogger<AuthCodeService> logger = Substitute.For<ILogger<AuthCodeService>>();

            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            AuthCodeService service = BuildAuthCodeService(owned: owned, logger: logger);

            OAuthRequest request = BuildOAuthRequest();
            Result<string> codeResult = await service.CreateAuthCodeAsync(request, new List<Claim>(), CancellationToken.None);
            string code = codeResult.Value;

            // First call: succeeds
            Result<AuthCode> firstResult = await service.ExchangeCodeAsync(code, CancellationToken.None);
            Assert.False(firstResult.IsError, "First call should succeed");

            // Second call: reuse attempt — repository logs Warning
            Result<AuthCode> secondResult = await service.ExchangeCodeAsync(code, CancellationToken.None);
            Assert.True(secondResult.IsError, "Second call (reuse) should return an error");
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, secondResult.Error.Code);
        }
    }
}
