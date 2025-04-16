using Bureau.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sven.Services
{
    public class SvenTokenProvider : ITokenProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly RsaSecurityKey _rsaKey;
        private readonly IStore<string, RefreshToken> _refreshTokenStore;
        private readonly TimeProvider _timeProvider;
        public SvenTokenProvider(IOptions<JwtOptions> jwtOptions, RsaSecurityKey rsaKey, IStore<string, RefreshToken> refreshTokenStore, TimeProvider timeProvider)
        {
            _jwtOptions = jwtOptions.Value;
            _rsaKey = rsaKey;
            _refreshTokenStore = refreshTokenStore;
            _timeProvider = timeProvider;
        }

        public async Task<Result<SvenToken>> CreateTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            Result<RefreshToken> storedRefreshTokenResult = await _refreshTokenStore.GetAsync(refreshToken, cancellationToken);

            if (storedRefreshTokenResult.IsError)
            {
                return storedRefreshTokenResult.Error;
            }
            Result<SvenToken> newAccessTokenResult = await CreateTokenAsync(storedRefreshTokenResult.Value, cancellationToken);

            await _refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

            return newAccessTokenResult;
        }

        public async Task<Result<SvenToken>> CreateTokenAsync(ClientClaims clientClaims, CancellationToken cancellationToken)
        {

            RefreshToken refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                ClientId = clientClaims.ClientId,
                Claims = clientClaims.Claims,
                ExpiresAt = _timeProvider.GetUtcNow().AddDays(30)
            };
            await _refreshTokenStore.StoreAsync(refreshToken.Token, refreshToken, cancellationToken);
            return CreateAccessToken(clientClaims.Claims, refreshToken.Token);
        }

        public async Task<Result<bool>> IsRefreshTokenValidAsync(string? refreshToken, string clientId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }
            Result<RefreshToken> storedRefreshTokenResult = await _refreshTokenStore.GetAsync(refreshToken, cancellationToken);

            return storedRefreshTokenResult.IsSuccess && storedRefreshTokenResult.Value.ClientId == clientId && storedRefreshTokenResult.Value.ExpiresAt > _timeProvider.GetUtcNow();
        }

        private SvenToken CreateAccessToken(List<Claim> claims, string refreshToken)
        {
            SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims.Append(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())),
                expires: _timeProvider.GetUtcNow().AddHours(1).DateTime,
                signingCredentials: creds
            );


            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new SvenToken(jwt, refreshToken);
        }
    }
}
