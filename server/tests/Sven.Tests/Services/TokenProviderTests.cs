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

        //[Fact]
        //public async Task IsRefreshTokenValidAsync_ValidToken_ReturnsTrue()
        //{
        //    // Arrange
        //    var validToken = new RefreshToken(
        //        "client1",
        //        "openid profile",
        //        DateTime.UtcNow.AddHours(1)
        //    );

        //    _refreshTokenStore.GetAsync("valid_token", Arg.Any<CancellationToken>())
        //        .Returns(Result<RefreshToken>.Success(validToken));

        //    // Act
        //    var result = await _provider.IsRefreshTokenValidAsync(
        //        "valid_token", "client1", "openid", CancellationToken.None);

        //    // Assert
        //    Assert.True(result.Value);
        //}

        //[Fact]
        //public async Task IsRefreshTokenValidAsync_TokenNotFound_ReturnsInvalidGrant()
        //{
        //    // Arrange
        //    _refreshTokenStore.GetAsync("missing_token", Arg.Any<CancellationToken>())
        //        .Returns(Result<RefreshToken>.Fail("not_found", "Token not found"));

        //    // Act
        //    var result = await _provider.IsRefreshTokenValidAsync(
        //        "missing_token", "client1", "openid", CancellationToken.None);

        //    // Assert
        //    Assert.True(result.IsError);
        //    Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.ErrorMessage);
        //    Assert.Equal(AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound, result.Error.LogMessage);
        //    _logger.Received(1).LogResultError(Arg.Any<ResultError>());
        //}

        //[Fact]
        //public async Task IsRefreshTokenValidAsync_ScopeExceeds_ReturnsInvalidScope()
        //{
        //    // Arrange
        //    var token = new RefreshToken("client1", "openid", DateTime.UtcNow.AddHours(1));
        //    _refreshTokenStore.GetAsync("token", Arg.Any<CancellationToken>())
        //        .Returns(Result<RefreshToken>.Success(token));

        //    // Act
        //    var result = await _provider.IsRefreshTokenValidAsync(
        //        "token", "client1", "openid profile", CancellationToken.None);

        //    // Assert
        //    Assert.True(result.IsError);
        //    Assert.Equal(AuthConstants.OAuth.Errors.InvalidScope, result.Error.ErrorMessage);
        //}

        //[Fact]
        //public async Task IsRefreshTokenValidAsync_ClientMismatch_ReturnsInvalidClient()
        //{
        //    // Arrange
        //    var token = new RefreshToken("client1", "openid", DateTime.UtcNow.AddHours(1));
        //    _refreshTokenStore.GetAsync("token", Arg.Any<CancellationToken>())
        //        .Returns(Result<RefreshToken>.Success(token));

        //    // Act
        //    var result = await _provider.IsRefreshTokenValidAsync(
        //        "token", "wrong_client", "openid", CancellationToken.None);

        //    // Assert
        //    Assert.True(result.IsError);
        //    Assert.Equal(AuthConstants.OAuth.Errors.InvalidClient, result.Error.ErrorMessage);
        //}

        //[Fact]
        //public async Task IsRefreshTokenValidAsync_ExpiredToken_ReturnsInvalidGrant()
        //{
        //    // Arrange
        //    var expiredToken = new RefreshToken(
        //        "client1",
        //        "openid",
        //        DateTime.UtcNow.AddHours(-1) // Already expired
        //    );

        //    _refreshTokenStore.GetAsync("expired_token", Arg.Any<CancellationToken>())
        //        .Returns(Result<RefreshToken>.Success(expiredToken));

        //    _timeProvider.GetUtcNow().Returns(DateTime.UtcNow);

        //    // Act
        //    var result = await _provider.IsRefreshTokenValidAsync(
        //        "expired_token", "client1", "openid", CancellationToken.None);

        //    // Assert
        //    Assert.True(result.IsError);
        //    Assert.Equal(AuthConstants.OAuth.Errors.InvalidGrant, result.Error.ErrorMessage);
        //}
    }

    // Test Helper Extension for Logger
    public static class LoggerExtensions
    {
        public static void LogResultError<T>(this ILogger<T> logger, ResultError error)
        {
            logger.LogError("Error: {Error} - {Description}",
                error.ErrorMessage, error.LogMessage);
        }
    }
}

