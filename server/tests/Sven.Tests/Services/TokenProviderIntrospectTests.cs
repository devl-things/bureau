using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Sven.Configurations;
using Sven.Data;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.TestUtils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Xunit;

namespace Sven.Tests.Services
{
    [Trait("Category", "Unit")]
    public class TokenProviderIntrospectTests
    {
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly IClientService _clientService;
        private readonly IHouseholdService _householdService;
        private readonly SvenTokenProvider _provider;
        private readonly RsaSecurityKey _rsaKey;
        private const string TestIssuer = "https://sven.test";

        public TokenProviderIntrospectTests()
        {
            _logger = Substitute.For<ILogger<SvenTokenProvider>>();
            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            RefreshTokenRepository refreshTokenRepository = RepositoryTestFactory.CreateRefreshTokenRepository(owned);
            _clientService = Substitute.For<IClientService>();
            _householdService = Substitute.For<IHouseholdService>();
            _rsaKey = new RsaSecurityKey(RSA.Create(2048));
            JwtOptions jwtOptions = new JwtOptions { Issuer = TestIssuer };
            _provider = new SvenTokenProvider(
                _logger,
                Options.Create(jwtOptions),
                _rsaKey,
                refreshTokenRepository,
                TimeProvider.System,
                _clientService,
                _householdService
            );
        }

        private string CreateToken(string issuer, DateTime? expires, string? subject, string? scope, string? clientId)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            if (subject != null)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
            }
            if (scope != null)
            {
                claims.Add(new Claim("scope", scope));
            }
            if (clientId != null)
            {
                claims.Add(new Claim("client_id", clientId));
            }

            SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                expires: expires ?? DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task IntrospectAsync_ExpiredToken_ReturnsActiveFalse()
        {
            string expiredToken = CreateToken(TestIssuer, DateTime.UtcNow.AddHours(-1), "user1", "openid", null);

            Result<IntrospectionResponse> result = await _provider.IntrospectAsync(expiredToken);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Active);
        }

        [Fact]
        public async Task IntrospectAsync_WrongIssuer_ReturnsActiveFalse()
        {
            string wrongIssuerToken = CreateToken("https://wrong.issuer", DateTime.UtcNow.AddHours(1), "user1", "openid", null);

            Result<IntrospectionResponse> result = await _provider.IntrospectAsync(wrongIssuerToken);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Active);
        }

        [Fact]
        public async Task IntrospectAsync_MalformedString_ReturnsActiveFalse()
        {
            Result<IntrospectionResponse> result = await _provider.IntrospectAsync("not-a-jwt");

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Active);
        }

        [Fact]
        public async Task IntrospectAsync_ValidToken_ReturnsActiveTrueWithClaims()
        {
            string validToken = CreateToken(TestIssuer, DateTime.UtcNow.AddHours(1), "user1", "openid profile", "client1");

            Result<IntrospectionResponse> result = await _provider.IntrospectAsync(validToken);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Active);
            Assert.Equal("user1", result.Value.Sub);
            Assert.Equal("openid profile", result.Value.Scope);
            Assert.Equal("client1", result.Value.ClientId);
            Assert.NotNull(result.Value.Jti);
            Assert.Equal(TestIssuer, result.Value.Iss);
            Assert.True(result.Value.Exp > 0);
        }

        [Fact]
        public async Task IntrospectAsync_MachineToken_ReturnsActiveTrueWithNoSub()
        {
            string machineToken = CreateToken(TestIssuer, DateTime.UtcNow.AddHours(1), null, "sven:read", "machine-client");

            Result<IntrospectionResponse> result = await _provider.IntrospectAsync(machineToken);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Active);
            Assert.Null(result.Value.Sub);
            Assert.Equal("sven:read", result.Value.Scope);
        }
    }
}
