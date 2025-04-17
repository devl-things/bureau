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
            string? refreshToken = null;
            if (clientClaims.HasScope(AuthConstants.Scopes.OfflineAccess))
            {
                RefreshToken refreshTokenObject = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString("N"),
                    ClientId = clientClaims.ClientId,
                    Claims = clientClaims.Claims,
                    Nonce = clientClaims.Nonce,
                    ExpiresAt = _timeProvider.GetUtcNow().AddDays(30)
                };
                await _refreshTokenStore.StoreAsync(refreshTokenObject.Token, refreshTokenObject, cancellationToken);
                refreshToken = refreshTokenObject.Token;
            }
            string? idToken = CreateIdToken(clientClaims);

            string accessToken = CreateAccessToken(clientClaims.Claims);

            return new SvenToken(accessToken, refreshToken, idToken);
        }

        private string? CreateIdToken(ClientClaims clientClaims)
        {
            if (clientClaims.HasScope(AuthConstants.Scopes.OpenId))
            {
                List<Claim> idClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Iss, _jwtOptions.Issuer),
                    new Claim(JwtRegisteredClaimNames.Sub, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Sub)),
                    new Claim(JwtRegisteredClaimNames.NameId, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Sub)),
                    new Claim(JwtRegisteredClaimNames.Aud, clientClaims.ClientId),
                    new Claim(JwtRegisteredClaimNames.Exp, _timeProvider.GetUtcNow().AddMinutes(5).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Iat, _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Acr, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Acr)),
                    new Claim(JwtRegisteredClaimNames.AuthTime, clientClaims.GetClaimValue(JwtRegisteredClaimNames.AuthTime)),

                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                if (!string.IsNullOrWhiteSpace(clientClaims.Nonce))
                {
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Nonce, clientClaims.Nonce));
                }

                if (clientClaims.HasScope(AuthConstants.Scopes.Profile))
                {
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Name, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Name)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.GivenName, clientClaims.GetClaimValue(JwtRegisteredClaimNames.GivenName)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, clientClaims.GetClaimValue(JwtRegisteredClaimNames.FamilyName)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Nickname, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Nickname)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Picture, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Picture)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.PreferredUsername, clientClaims.GetClaimValue(JwtRegisteredClaimNames.PreferredUsername)));
                }

                if (clientClaims.HasScope(AuthConstants.Scopes.Email))
                {
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Email, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Email)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.EmailVerified, clientClaims.GetClaimValue(JwtRegisteredClaimNames.EmailVerified)));
                }

                if (clientClaims.HasScope(AuthConstants.Scopes.Address))
                {
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.Address, clientClaims.GetClaimValue(JwtRegisteredClaimNames.Address)));
                }

                if (clientClaims.HasScope(AuthConstants.Scopes.Phone))
                {
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.PhoneNumber, clientClaims.GetClaimValue(JwtRegisteredClaimNames.PhoneNumber)));
                    idClaims.Add(new Claim(JwtRegisteredClaimNames.PhoneNumberVerified, clientClaims.GetClaimValue(JwtRegisteredClaimNames.PhoneNumberVerified)));
                }

                SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
                JwtSecurityToken idToken = new JwtSecurityToken(
                    claims: idClaims,
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(idToken);
            }
            return null;
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

        private string CreateAccessToken(List<Claim> claims)
        {
            SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims.Append(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())),
                expires: _timeProvider.GetUtcNow().AddHours(1).DateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
