using Bureau.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Cryptography;

namespace Sven.Tests.Services
{
    public class TokenProviderTests
    {
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly IStore<string, RefreshToken> _refreshTokenStore;
        private readonly TimeProvider _timeProvider;
        private readonly SvenTokenProvider _provider;

        private readonly string _clientId = "client1";

        public TokenProviderTests()
        {
            _logger = Substitute.For<ILogger<SvenTokenProvider>>();
            _refreshTokenStore = Substitute.For<IStore<string, RefreshToken>>();
            _timeProvider = Substitute.For<TimeProvider>();
            _provider = new SvenTokenProvider(
                _logger,
                Options.Create(new JwtOptions()), // Mock JwtOptions
                new RsaSecurityKey(RSA.Create(2048)), // Mock RSA key
                _refreshTokenStore,
                _timeProvider
            );
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()
        {
            RefreshToken validToken = new RefreshToken()
            {
                ClientId = _clientId,
                Scope = $"{AuthConstants.Scopes.OpenId} {AuthConstants.Scopes.Profile}",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _refreshTokenStore.GetAsync("valid_token", Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(validToken));

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "valid_token", _clientId, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_TokenNotFound_ReturnsInvalidGrant()
        {
            ResultError resultErrorExpected = new ResultError(AuthConstants.OAuth.Errors.InvalidGrant, AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound);
            _refreshTokenStore.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(resultErrorExpected));

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "missing_token", _clientId, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.ErrorMessage);
            Assert.Equal(AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound, result.Error.LogMessage);
            _logger.Received(1).LogResultError(resultErrorExpected);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ScopeExceeds_ReturnsInvalidScope()
        {
            RefreshToken token = new RefreshToken()
            {
                ClientId = _clientId,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _refreshTokenStore.GetAsync("token", Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(token));

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "token", _clientId, $"{AuthConstants.Scopes.OpenId} {AuthConstants.Scopes.Profile}", CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidScope, result.Error.ErrorMessage);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ClientMismatch_ReturnsInvalidClient()
        {
            RefreshToken token = new RefreshToken()
            {
                ClientId = _clientId,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _refreshTokenStore.GetAsync("token", Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(token));

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "token", "wrong_client", AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidClient, result.Error.ErrorMessage);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ExpiredToken_ReturnsInvalidGrant()
        {
            RefreshToken token = new RefreshToken()
            {
                ClientId = _clientId,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            _refreshTokenStore.GetAsync("token", Arg.Any<CancellationToken>())
                .Returns(new Result<RefreshToken>(token));
            _timeProvider.GetUtcNow().Returns(DateTime.UtcNow);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "token", _clientId, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.ErrorMessage);
        }
    }
}

