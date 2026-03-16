// TODO: IntrospectAsync not yet on ITokenProvider — added in 03-02.
// These stubs compile without the interface method by using Assert.Fail bodies only.
// When Plan 03-02 adds ITokenProvider.IntrospectAsync and IntrospectionResponse,
// replace Assert.Fail with the real assertions shown in each TODO comment.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace Sven.Tests.Services
{
    [Trait("Category", "Unit")]
    public class TokenProviderIntrospectTests
    {
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly IStore<string, RefreshToken> _refreshTokenStore;
        private readonly IClientService _clientService;
        private readonly SvenTokenProvider _provider;

        public TokenProviderIntrospectTests()
        {
            _logger = Substitute.For<ILogger<SvenTokenProvider>>();
            _refreshTokenStore = Substitute.For<IStore<string, RefreshToken>>();
            IStore<string, RefreshToken> refreshTokenStore = _refreshTokenStore;
            _clientService = Substitute.For<IClientService>();
            _provider = new SvenTokenProvider(
                _logger,
                Options.Create(new JwtOptions()),
                new RsaSecurityKey(RSA.Create(2048)),
                refreshTokenStore,
                TimeProvider.System,
                _clientService
            );
        }

        [Fact]
        public async Task IntrospectAsync_ExpiredToken_ReturnsActiveFalse()
        {
            // ValidTo in the past → IntrospectionResponse { Active = false }
            Assert.Fail("not implemented");
            // TODO: create expired JWT (ValidTo = DateTime.UtcNow.AddHours(-1));
            //       call provider.IntrospectAsync(expiredToken);
            //       assert result.Active == false.
        }

        [Fact]
        public async Task IntrospectAsync_WrongIssuer_ReturnsActiveFalse()
        {
            // JWT with issuer != _jwtOptions.Issuer → { Active = false }
            Assert.Fail("not implemented");
            // TODO: create JWT with Issuer = "https://wrong.issuer";
            //       call provider.IntrospectAsync(wrongIssuerToken);
            //       assert result.Active == false.
        }

        [Fact]
        public async Task IntrospectAsync_MalformedString_ReturnsActiveFalse()
        {
            // ReadJwtToken throws → { Active = false }
            Assert.Fail("not implemented");
            // TODO: pass "not-a-jwt" to provider.IntrospectAsync;
            //       assert result.Active == false.
        }

        [Fact]
        public async Task IntrospectAsync_ValidToken_ReturnsActiveTrueWithClaims()
        {
            // well-formed non-expired JWT from test issuer → Active = true, Sub, Scope, Jti, Iss populated
            Assert.Fail("not implemented");
            // TODO: create valid JWT with correct issuer and future ValidTo;
            //       call provider.IntrospectAsync(validToken);
            //       assert result.Active == true, result.Sub != null, result.Scope != null,
            //       result.Jti != null, result.Iss == testIssuer.
        }

        [Fact]
        public async Task IntrospectAsync_MachineToken_ReturnsActiveTrueWithNoSub()
        {
            // machine token (no sub claim) → Active = true, Sub null
            Assert.Fail("not implemented");
            // TODO: create machine token JWT (no sub claim) with correct issuer;
            //       call provider.IntrospectAsync(machineToken);
            //       assert result.Active == true, result.Sub == null.
        }
    }
}
