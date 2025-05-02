using Bureau.Core;
using Bureau.Core.Extensions;
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
        private readonly ILogger<SvenTokenProvider> _logger;
        private readonly JwtOptions _jwtOptions;
        private readonly IClientProvider _clientProvider;
        private readonly RsaSecurityKey _rsaKey;
        private readonly IStore<string, RefreshToken> _refreshTokenStore;
        private readonly TimeProvider _timeProvider;

        private Client? _currentClient;
        private readonly TokenLifetimeOptions _tokenLifetimeOptions;
        public SvenTokenProvider(ILogger<SvenTokenProvider> logger, IOptions<JwtOptions> jwtOptions,
            RsaSecurityKey rsaKey, IStore<string, RefreshToken> refreshTokenStore, TimeProvider timeProvider, IClientProvider clientProvider)
        {
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
            _tokenLifetimeOptions = new TokenLifetimeOptions(_jwtOptions);
            _rsaKey = rsaKey;
            _refreshTokenStore = refreshTokenStore;
            _timeProvider = timeProvider;
            _clientProvider = clientProvider;
            _currentClient = null;
        }

        public async Task<Result<SvenToken>> CreateTokenAsync(string refreshToken, string? scope, CancellationToken cancellationToken = default)
        {
            Result<RefreshToken> storedRefreshTokenResult = await _refreshTokenStore.GetAsync(refreshToken, cancellationToken);

            if (storedRefreshTokenResult.IsError)
            {
                _logger.LogResultError(storedRefreshTokenResult.Error);
                return new ResultError(AuthConstants.OAuth.Errors.InvalidGrant, "Refresh token not found.");
            }
            await SetTokenLifetimeOptions(storedRefreshTokenResult.Value, cancellationToken);
            // create a new refresh token
            Result<string?> newRefreshTokenResult = await CreateRefreshTokenAsync(storedRefreshTokenResult.Value, cancellationToken);
            if (newRefreshTokenResult.IsError)
            {
                return newRefreshTokenResult.Error;
            }
            Result removeResult = await _refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);
            if (removeResult.IsError)
            {
                _logger.LogResultError(storedRefreshTokenResult.Error);
            }

            ClientClaims newClientClaims = new ClientClaims(storedRefreshTokenResult.Value, scope);
            // id token should be explicitly requested in refresh flow
            string? idToken = CreateIdToken(newClientClaims);

            string accessToken = string.IsNullOrWhiteSpace(scope) ?
                CreateAccessToken(storedRefreshTokenResult.Value) : CreateAccessToken(newClientClaims);

            return new SvenToken(accessToken, newRefreshTokenResult.Value, idToken);
        }

        public async Task<Result<SvenToken>> CreateTokenAsync(ClientClaims clientClaims, CancellationToken cancellationToken = default)
        {
            await SetTokenLifetimeOptions(clientClaims, cancellationToken);
            Result<string?> refreshTokenResult = await CreateRefreshTokenAsync(clientClaims, cancellationToken);
            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Error;
            }

            string? idToken = CreateIdToken(clientClaims);

            string accessToken = CreateAccessToken(clientClaims);

            return new SvenToken(accessToken, refreshTokenResult.Value, idToken);
        }

        private async Task SetTokenLifetimeOptions(ClientClaims clientClaims, CancellationToken cancellationToken)
        {
            if (_currentClient == null || _currentClient.Identifier != clientClaims.ClientId)
            {
                Result<Client> clientResult = await _clientProvider.GetClientAsync(clientClaims.ClientId, cancellationToken);
                if (clientResult.IsError)
                {
                    _logger.LogResultError(clientResult.Error);
                    return;
                }
                _currentClient = clientResult.Value;
            }
            if (_currentClient.RefreshTokenLifetime.HasValue)
            {
                _tokenLifetimeOptions.RefreshTokenLifetime = _currentClient.RefreshTokenLifetime.Value;
            }
            if (_currentClient.IdTokenLifetime.HasValue)
            {
                _tokenLifetimeOptions.IdTokenLifetime = _currentClient.IdTokenLifetime.Value;
            }
            if (_currentClient.AccessTokenLifetime.HasValue)
            {
                _tokenLifetimeOptions.AccessTokenLifetime = _currentClient.AccessTokenLifetime.Value;
            }
        }

        private async Task<Result<string?>> CreateRefreshTokenAsync(ClientClaims clientClaims, CancellationToken cancellationToken)
        {
            string? refreshToken = null;
            if (clientClaims.HasScope(AuthConstants.Scopes.OfflineAccess))
            {
                RefreshToken refreshTokenObject = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString("N"),
                    ClientId = clientClaims.ClientId,
                    RedirectUri = clientClaims.RedirectUri,
                    Claims = clientClaims.Claims,
                    Scope = clientClaims.Scope,
                    Nonce = clientClaims.Nonce,
                    ExpiresAt = _timeProvider.GetFutureTime(_tokenLifetimeOptions.RefreshTokenLifetime)
                };
                Result storeResult = await _refreshTokenStore.StoreAsync(refreshTokenObject.Token, refreshTokenObject, cancellationToken);
                if (storeResult.IsError)
                {
                    _logger.LogResultError(storeResult.Error);
                    return new ResultError(AuthConstants.OAuth.Errors.ServerError, "Failed create token.");
                }
                refreshToken = refreshTokenObject.Token;
            }
            return new Result<string?>(refreshToken);
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
                    new Claim(JwtRegisteredClaimNames.Exp, _timeProvider.GetFutureUnixTimeSeconds(_tokenLifetimeOptions.IdTokenLifetime).ToString(), ClaimValueTypes.Integer64),
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

        public async Task<Result<bool>> IsRefreshTokenValidAsync(string refreshToken, string clientId, string redirectUri, string? scope, CancellationToken cancellationToken = default)
        {
            Result<RefreshToken> storedRefreshTokenResult = await _refreshTokenStore.GetAsync(refreshToken, cancellationToken);

            if (storedRefreshTokenResult.IsError)
            {
                _logger.LogResultError(storedRefreshTokenResult.Error);
                return new ResultError(AuthConstants.OAuth.Errors.InvalidGrant, AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound);
            }
            RefreshToken storedToken = storedRefreshTokenResult.Value;
            if (!storedToken.IsScopeSameOrSubset(scope))
            {
                return new ResultError(AuthConstants.OAuth.Errors.InvalidScope, AuthConstants.OAuth.ErrorDescriptions.RequestedScopeExceedsGranted);
            }
            if (storedToken.ClientId != clientId || storedToken.RedirectUri != redirectUri)
            {
                return new ResultError(AuthConstants.OAuth.Errors.InvalidClient, "Refresh token does not belong to this client.");
            }
            if (storedToken.ExpiresAt <= _timeProvider.GetUtcNow())
            {
                return new ResultError(AuthConstants.OAuth.Errors.InvalidGrant, "Refresh token has expired.");
            }

            return true;
        }

        private string CreateAccessToken(ClientClaims clientClaims)
        {
            SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: clientClaims.Claims.Append(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())),
                expires: _timeProvider.GetFutureTime(_tokenLifetimeOptions.AccessTokenLifetime).DateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result<bool>> RevokeAsync(string token, string clientId, string? tokenTypeHint, CancellationToken cancellationToken = default)
        {
            Result<RefreshToken> storedRefreshTokenResult = await _refreshTokenStore.GetAsync(token, cancellationToken);

            if (storedRefreshTokenResult.IsError)
            {
                _logger.LogResultError(storedRefreshTokenResult.Error);
                return new Result<bool>(false);
            }
            if (storedRefreshTokenResult.Value.ClientId != clientId)
            {
                _logger.LogWarning("Token's client not the same as received client");
                return new Result<bool>(false);
            }

            Result removeResult = await _refreshTokenStore.RemoveAsync(token, cancellationToken);
            if (removeResult.IsError)
            {
                _logger.LogResultError(storedRefreshTokenResult.Error);
                return new ResultError(AuthConstants.OAuth.Errors.ServerError, "Token couldn't be revoked.");
            }

            return true;
        }
    }
}
