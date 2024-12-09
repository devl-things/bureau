using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Constants;
using Bureau.Identity.Managers;
using Bureau.Identity.Mappers;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Bureau.Google.Managers
{
    internal class GoogleAccessTokenManager : IGoogleAccessTokenManager
    {
        //PERHAPS: get in options
        private static TimeSpan s_accessTokenRefreshWindow = TimeSpan.FromMinutes(5);

        private readonly IUserManager _userManager;
        private readonly TimeProvider _timeProvider;
        //TODO change remove GoogleOptions
        private readonly IOptionsMonitor<GoogleOptions> _options;
        IHttpClientFactory _clientFactory;
        public GoogleAccessTokenManager(IUserManager userManager, TimeProvider timeProvider, IOptionsMonitor<GoogleOptions> options, IHttpClientFactory clientFactory)
        {
            _userManager = userManager;
            _timeProvider = timeProvider;
            _options = options;
            _clientFactory = clientFactory;
        }

        public async Task<Result<AuthenticationHeaderValue>> GetAuthenticationHeaderAsync(BureauUserLoginModel userLogin, CancellationToken cancellationToken = default)
        {
            GetUserLoginTokensRequest userLoginRequest = userLogin.ToGetUserLoginTokensRequest();
            Result<Dictionary<string, string?>> authTokensResult = await _userManager.GetUserLoginTokensAsync(userLoginRequest, cancellationToken);

            if (authTokensResult.IsError) 
            {
                return authTokensResult.Error;
            }

            if (!authTokensResult.Value.TryGetValue(OpenIdConnectParameterNames.TokenType, out string? tokenType) || string.IsNullOrWhiteSpace(tokenType))
            {
                return ErrorMessages.BureauVariableUndefined.Format(OpenIdConnectParameterNames.TokenType);
            }

            if (!authTokensResult.Value.TryGetValue(OpenIdConnectParameterNames.AccessToken, out string? accessToken) || string.IsNullOrWhiteSpace(accessToken))
            {
                return ErrorMessages.BureauVariableUndefined.Format(OpenIdConnectParameterNames.AccessToken);
            }

            if (!authTokensResult.Value.TryGetValue(BureauIdentityConstants.ExpiresAtTokenName, out string? expiresAtUtcStr) || string.IsNullOrWhiteSpace(expiresAtUtcStr) ||
                !DateTime.TryParseExact(expiresAtUtcStr, "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime expiresAtUtc)) 
            {
                return ErrorMessages.BureauVariableUndefined.Format(BureauIdentityConstants.ExpiresAtTokenName);
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            if (expiresAtUtc - s_accessTokenRefreshWindow < now)
            {
                if (!authTokensResult.Value.TryGetValue(OpenIdConnectParameterNames.RefreshToken, out string? refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    return ErrorMessages.BureauVariableUndefined.Format(OpenIdConnectParameterNames.RefreshToken); ;
                }

                GoogleOptions options = _options.CurrentValue;
                
                FormUrlEncodedContent refreshContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { BureauIdentityConstants.ClientIdPropertyName, options.ClientId },
                    { BureauIdentityConstants.ClientSecretPropertyName, options.ClientSecret },
                    { BureauIdentityConstants.GrantTypePropertyName, BureauIdentityConstants.GrantTypeRefreshTokenPropertyValue },
                    { BureauIdentityConstants.RefreshTokenPropertyName, refreshToken },
                });
                
                try
                {
                    HttpResponseMessage refreshResponse = await _clientFactory.CreateClient().PostAsync(options.TokenEndpoint, refreshContent, cancellationToken);

                    if (!refreshResponse.IsSuccessStatusCode) 
                    {
                        return ErrorMessages.BureauUnsuccessfulHttpResponse.Format("Token refresh",refreshResponse.StatusCode, refreshResponse.ReasonPhrase, string.Empty);
                    }

                    string jsonString = await refreshResponse.Content.ReadAsStringAsync(cancellationToken);

                    UpdateUserLoginTokensRequest updateRequest = new UpdateUserLoginTokensRequest(userLoginRequest, new Dictionary<string, string>());
                    using (JsonDocument doc = JsonDocument.Parse(jsonString)) 
                    {
                        JsonElement root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (!root.TryGetProperty(BureauIdentityConstants.AccessTokenPropertyName, out JsonElement accessTokenProperty)) 
                            {
                                return ErrorMessages.BureauVariableUndefined.Format(BureauIdentityConstants.AccessTokenPropertyName);
                            }
                            accessToken = accessTokenProperty.GetString();

                            if (root.TryGetProperty(BureauIdentityConstants.RefreshTokenPropertyName, out JsonElement refreshTokenProperty))
                            {
                                updateRequest.Tokens.Add(OpenIdConnectParameterNames.RefreshToken, refreshTokenProperty.GetString()!);
                            }

                            if (!root.TryGetProperty(BureauIdentityConstants.ExpiresInPropertyName, out JsonElement expiresInTokenProperty) ||
                                !expiresInTokenProperty.TryGetInt32(out int expiresInSeconds))
                            {
                                return ErrorMessages.BureauVariableUndefined.Format(BureauIdentityConstants.ExpiresInPropertyName);
                            }
                            updateRequest.Tokens.Add(BureauIdentityConstants.ExpiresAtTokenName, now.AddSeconds(expiresInSeconds).ToString("o"));
                        }                    
                    }

                    if (string.IsNullOrWhiteSpace(accessToken)) 
                    { 
                        return ErrorMessages.BureauVariableUndefined.Format(BureauIdentityConstants.AccessTokenPropertyName); 
                    }
                    updateRequest.Tokens.Add(OpenIdConnectParameterNames.AccessToken, accessToken);                   

                    if(updateRequest.Tokens.TryGetValue(OpenIdConnectParameterNames.RefreshToken, out string? refreshTokenNew) &&
                       string.IsNullOrWhiteSpace(refreshTokenNew)) 
                    {
                        updateRequest.Tokens.Remove(OpenIdConnectParameterNames.RefreshToken);
                    }

                    Result result = await _userManager.UpdateUserLoginTokensAsync(updateRequest, cancellationToken);
                    if (result.IsError) 
                    {
                        return result.Error;
                    }
                }
                catch (Exception ex)
                {
                    return new ResultError(ErrorMessages.BureauActionFailed.Format(nameof(GetAuthenticationHeaderAsync)), ex);
                }
                //TODO maybe this is not needed, lets see in testing
                // Sign-in, to store the refreshed auth tokens (often stored into a cookie).
                //await httpContext.SignInAsync(options.SignInScheme, auth.Principal, auth.Properties);
            }

            return new AuthenticationHeaderValue(tokenType, accessToken);
        }
    }
}
