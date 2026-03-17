using Bureau;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Data;
using Sven.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sven.Services
{
    internal sealed class TokenExchangeService : ITokenExchangeService
    {
        private readonly IClientService _clientService;
        private readonly IExternalTokenService _externalTokenService;
        private readonly IHouseholdService _householdService;
        private readonly IExternalTokenRefresher _externalTokenRefresher;
        private readonly JwtOptions _jwtOptions;
        private readonly RsaSecurityKey _rsaKey;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<TokenExchangeService> _logger;

        public TokenExchangeService(
            IClientService clientService,
            IExternalTokenService externalTokenService,
            IHouseholdService householdService,
            IExternalTokenRefresher externalTokenRefresher,
            IOptions<JwtOptions> jwtOptions,
            RsaSecurityKey rsaKey,
            TimeProvider timeProvider,
            ILogger<TokenExchangeService> logger)
        {
            _clientService = clientService;
            _externalTokenService = externalTokenService;
            _householdService = householdService;
            _externalTokenRefresher = externalTokenRefresher;
            _jwtOptions = jwtOptions.Value;
            _rsaKey = rsaKey;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<Result<TokenExchangeResponse>> ExchangeAsync(
            string clientId,
            string subjectToken,
            string provider,
            string featureKey,
            CancellationToken cancellationToken = default)
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();

            // Step 1: Validate client
            Result<Client> clientResult = await _clientService.GetClientAsync(clientId, cancellationToken);
            if (clientResult.IsError || !clientResult.Value.Active)
            {
                _logger.LogWarning(
                    "TokenExchange step=1 failure: ClientId={ClientId} Reason=unknown_client Timestamp={Timestamp} Result=failure",
                    clientId, now);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidClient, "Unknown or inactive client.");
            }

            Client client = clientResult.Value;

            // Step 2: Check bureau_features allowlist
            if (client.BureauFeatures == null || !client.BureauFeatures.ContainsKey(featureKey))
            {
                _logger.LogWarning(
                    "TokenExchange step=2 failure: ClientId={ClientId} FeatureKey={FeatureKey} Reason=feature_not_in_allowlist Timestamp={Timestamp} Result=failure",
                    clientId, featureKey, now);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidScope, "Requested feature not in client bureau_features allowlist.");
            }

            // Step 3: Validate subject_token JWT
            JwtSecurityToken jwt;
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = _rsaKey,
                    ClockSkew = TimeSpan.Zero,
                };
                handler.ValidateToken(subjectToken, validationParams, out SecurityToken validatedToken);
                jwt = (JwtSecurityToken)validatedToken;
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
            {
                _logger.LogWarning(
                    "TokenExchange step=3 failure: ClientId={ClientId} Reason=invalid_subject_token Timestamp={Timestamp} Result=failure Message={Message}",
                    clientId, now, ex.Message);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "Subject token is invalid or expired.");
            }

            // Extract userId from sub claim
            string userId = jwt.Subject ?? string.Empty;

            // Step 4: Check scope claim contains featureKey
            Claim? scopeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "scope");
            string scopeValue = scopeClaim?.Value ?? string.Empty;
            if (!ContainsFeatureKey(scopeValue, featureKey))
            {
                _logger.LogWarning(
                    "TokenExchange step=4 failure: ClientId={ClientId} UserId={UserId} FeatureKey={FeatureKey} Reason=feature_not_in_user_claims Timestamp={Timestamp} Result=failure",
                    clientId, userId, featureKey, now);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "Feature key not present in subject token claims.");
            }

            // Step 5: Gather tokens — user's own token and household shared tokens
            Result<UserExternalToken> ownTokenResult = await _externalTokenService.GetByUserAndProviderAsync(userId, provider, cancellationToken);

            Result<HouseholdMembership> membershipResult = await _householdService.GetMembershipAsync(userId, cancellationToken);

            IReadOnlyList<SharedExternalTokenConsent> filteredSharedConsents = [];
            if (!membershipResult.IsError)
            {
                Result<IReadOnlyList<SharedExternalTokenConsent>> sharedTokensResult =
                    await _householdService.GetAllActiveSharedTokensAsync(
                        membershipResult.Value.HouseholdId, provider, featureKey, cancellationToken);

                if (!sharedTokensResult.IsError)
                {
                    List<SharedExternalTokenConsent> filtered = new List<SharedExternalTokenConsent>();
                    foreach (SharedExternalTokenConsent consent in sharedTokensResult.Value)
                    {
                        if (consent.OwnerUserId != userId)
                        {
                            filtered.Add(consent);
                        }
                    }
                    filteredSharedConsents = filtered;
                }
            }

            bool hasOwnToken = !ownTokenResult.IsError;
            if (!hasOwnToken && filteredSharedConsents.Count == 0)
            {
                _logger.LogWarning(
                    "TokenExchange step=5 failure: ClientId={ClientId} UserId={UserId} Provider={Provider} FeatureKey={FeatureKey} Reason=no_token_available Timestamp={Timestamp} Result=failure",
                    clientId, userId, provider, featureKey, now);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "No external token available for the requested provider and feature.");
            }

            // Step 6: Refresh near-expiry or reauth-required tokens
            DateTimeOffset refreshThreshold = now.AddSeconds(60);

            if (hasOwnToken)
            {
                UserExternalToken ownToken = ownTokenResult.Value;
                if (ownToken.RequiresReauthorisation || ownToken.ExpiresAt <= refreshThreshold)
                {
                    try
                    {
                        await _externalTokenRefresher.RefreshAsync(ownToken, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "TokenExchange step=6 failure: ClientId={ClientId} UserId={UserId} Provider={Provider} Reason=refresh_failed Timestamp={Timestamp} Result=failure",
                            clientId, userId, provider, now);
                        return ResultError.From(AuthConstants.OAuth.Errors.ServerError, "Failed to refresh external token.");
                    }
                }
            }

            // For shared tokens we need to load the actual UserExternalToken to refresh and return
            List<UserExternalToken> sharedTokens = new List<UserExternalToken>();
            foreach (SharedExternalTokenConsent consent in filteredSharedConsents)
            {
                Result<UserExternalToken> sharedTokenResult = await _externalTokenService.GetByUserAndProviderAsync(
                    consent.OwnerUserId, provider, cancellationToken);
                if (sharedTokenResult.IsError)
                {
                    continue;
                }

                UserExternalToken sharedToken = sharedTokenResult.Value;
                if (sharedToken.RequiresReauthorisation || sharedToken.ExpiresAt <= refreshThreshold)
                {
                    try
                    {
                        await _externalTokenRefresher.RefreshAsync(sharedToken, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "TokenExchange step=6 failure (shared): ClientId={ClientId} OwnerUserId={OwnerUserId} Provider={Provider} Reason=refresh_failed Timestamp={Timestamp} Result=failure",
                            clientId, consent.OwnerUserId, provider, now);
                        return ResultError.From(AuthConstants.OAuth.Errors.ServerError, "Failed to refresh shared external token.");
                    }
                }

                sharedTokens.Add(sharedToken);
            }

            // Build response
            TokenExchangeResponse response = new TokenExchangeResponse();

            if (hasOwnToken)
            {
                UserExternalToken ownToken = ownTokenResult.Value;
                response.AccessToken = ownToken.AccessToken;
                int expiresInSeconds = (int)(ownToken.ExpiresAt - now).TotalSeconds;
                response.ExpiresIn = expiresInSeconds > 0 ? expiresInSeconds : 0;

                _logger.LogInformation(
                    "TokenExchange token issued: Type=own ClientId={ClientId} UserId={UserId} Provider={Provider} FeatureKey={FeatureKey} ExpiresAt={ExpiresAt} Timestamp={Timestamp}",
                    clientId, userId, provider, featureKey, ownToken.ExpiresAt, now);
            }

            if (sharedTokens.Count > 0)
            {
                List<BureauTokenEntry> bureauTokens = new List<BureauTokenEntry>();
                foreach (UserExternalToken sharedToken in sharedTokens)
                {
                    int sharedExpiresIn = (int)(sharedToken.ExpiresAt - now).TotalSeconds;
                    bureauTokens.Add(new BureauTokenEntry
                    {
                        AccessToken = sharedToken.AccessToken,
                        ExpiresIn = sharedExpiresIn > 0 ? sharedExpiresIn : 0,
                        SourceUserId = sharedToken.UserId,
                        Provider = sharedToken.Provider,
                    });

                    _logger.LogInformation(
                        "TokenExchange token issued: Type=shared ClientId={ClientId} UserId={UserId} SourceUserId={SourceUserId} Provider={Provider} FeatureKey={FeatureKey} ExpiresAt={ExpiresAt} Timestamp={Timestamp}",
                        clientId, userId, sharedToken.UserId, provider, featureKey, sharedToken.ExpiresAt, now);
                }
                response.BureauTokens = bureauTokens;
            }

            return response;
        }

        private static bool ContainsFeatureKey(string scopeValue, string featureKey)
        {
            string[] parts = scopeValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (string.Equals(part, featureKey, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
