using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Sven.Configurations;
using Sven.Data;
using Sven.Data.Repositories;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using Sven.Tests.TestUtils;
using System.Security.Cryptography;

namespace Sven.Tests.Services
{
    public class TokenProviderTests
    {
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly TimeProvider _timeProvider;
        private readonly IClientService _clientService;
        private readonly IHouseholdService _householdService;
        private readonly SvenTokenProvider _provider;

        private readonly string _clientId = "client1";
        private readonly string _redirectUri = "http://localhost";

        public TokenProviderTests()
        {
            _logger = Substitute.For<ILogger<SvenTokenProvider>>();
            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            _refreshTokenRepository = RepositoryTestFactory.CreateRefreshTokenRepository(owned);
            _timeProvider = TimeProvider.System;
            _clientService = Substitute.For<IClientService>();
            _householdService = Substitute.For<IHouseholdService>();
            _provider = new SvenTokenProvider(
                _logger,
                Options.Create(new JwtOptions()), // Mock JwtOptions
                new RsaSecurityKey(RSA.Create(2048)), // Mock RSA key
                _refreshTokenRepository,
                _timeProvider,
                _clientService,
                _householdService
            );
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()
        {
            RefreshToken validToken = new RefreshToken()
            {
                Token = "valid_token",
                ClientId = _clientId,
                RedirectUri = _redirectUri,
                Scope = $"{AuthConstants.Scopes.OpenId} {AuthConstants.Scopes.Profile}",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            };
            await _refreshTokenRepository.StoreAsync(validToken, CancellationToken.None);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "valid_token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_TokenNotFound_ReturnsInvalidGrant()
        {
            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "missing_token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.Code);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ScopeExceeds_ReturnsInvalidScope()
        {
            RefreshToken token = new RefreshToken()
            {
                Token = "scope-token",
                ClientId = _clientId,
                RedirectUri = _redirectUri,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            };
            await _refreshTokenRepository.StoreAsync(token, CancellationToken.None);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "scope-token", _clientId, _redirectUri, $"{AuthConstants.Scopes.OpenId} {AuthConstants.Scopes.Profile}", CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidScope, result.Error.Code);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ClientMismatch_ReturnsInvalidClient()
        {
            RefreshToken token = new RefreshToken()
            {
                Token = "client-mismatch-token",
                ClientId = _clientId,
                RedirectUri = _redirectUri,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            };
            await _refreshTokenRepository.StoreAsync(token, CancellationToken.None);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "client-mismatch-token", "wrong_client", _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidClient, result.Error.Code);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_RedirectUriMismatch_ReturnsInvalidClient()
        {
            RefreshToken token = new RefreshToken()
            {
                Token = "redirect-mismatch-token",
                ClientId = _clientId,
                RedirectUri = _redirectUri,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            };
            await _refreshTokenRepository.StoreAsync(token, CancellationToken.None);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "redirect-mismatch-token", _clientId, "https://wronguri", AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidClient, result.Error.Code);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task IsRefreshTokenValidAsync_ExpiredToken_ReturnsInvalidGrant()
        {
            RefreshToken token = new RefreshToken()
            {
                Token = "expired-token",
                ClientId = _clientId,
                RedirectUri = _redirectUri,
                Scope = AuthConstants.Scopes.OpenId,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
            };
            await _refreshTokenRepository.StoreAsync(token, CancellationToken.None);

            Result<bool> result = await _provider.IsRefreshTokenValidAsync(
                "expired-token", _clientId, _redirectUri, AuthConstants.Scopes.OpenId, CancellationToken.None);

            Assert.True(result.IsError);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.Code);
        }
    }
}
