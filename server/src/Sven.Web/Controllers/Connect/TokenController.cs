using Bureau;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.Models;
using Sven.Services;

namespace Sven.Controllers.Connect
{
    [ApiController]
    [Route(Endpoints.Connect.Token)]
    [EnableRateLimiting("token-endpoint")]
    public class TokenController : ConnectController
    {
        private readonly IAuthCodeService _authCodeManager;
        private readonly ITokenProvider _tokenProvider;

        public TokenController(ILogger<TokenController> logger, ITokenProvider tokenProvider,
            IAuthCodeService authCodeManager) : base(logger)
        {
            _authCodeManager = authCodeManager;
            _tokenProvider = tokenProvider;
        }
        [HttpPost]
        [DisableAutoValidation]
        [ServiceFilter(typeof(OAuthValidationFilter))]
        public async Task<IActionResult> TokenAsync([FromForm] TokenRequest request, CancellationToken cancellationToken = default)
        {
            if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantTypes.AuthorizationCode))
            {
                return await HandleAuthorizationCodeFlow(request, cancellationToken);
            }
            else if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantTypes.RefreshToken))
            {
                return await HandleRefreshTokenFlow(request, cancellationToken);
            }
            else if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantTypes.ClientCredentials))
            {
                return await HandleClientCredentialsFlow(request, cancellationToken);
            }
            else if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantTypes.TokenExchange))
            {
                return await HandleTokenExchangeFlow(request, cancellationToken);
            }

            return OAuthError(AuthConstants.OAuth.Errors.UnsupportedGrantType, AuthConstants.OAuth.ErrorDescriptions.UnsupportedGrantType);
        }

        private async Task<IActionResult> HandleRefreshTokenFlow(TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "Refresh token is missing.");
            }
            Result<bool> isValidResult = await _tokenProvider.IsRefreshTokenValidAsync(request.RefreshToken!, request.ClientId, request.RedirectUri, request.Scope, cancellationToken);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return isValidResult.Error.ToOAuthError();
            }

            Result<SvenToken> jwtResult = await _tokenProvider.CreateTokenAsync(request.RefreshToken!, request.Scope, cancellationToken);

            return HandleTokenResult(jwtResult);
        }

        private async Task<IActionResult> HandleAuthorizationCodeFlow(TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.CodeVerifier))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.CodeOrCodeVerifierMissing);
            }

            Result<AuthCode> authCodeResult = await _authCodeManager.GetAuthCodeAsync(request.Code!, cancellationToken);
            if (authCodeResult.IsError)
            {
                _logger.LogResultError(authCodeResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Invalid or expired authorization code.");
            }

            if (!authCodeResult.Value.IsClientValid(request.ClientId, request.RedirectUri))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Client not recognized.");
            }

            if (!_authCodeManager.IsAuthCodeExpired(authCodeResult.Value))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Code verifier mismatch.");
            }

            if (!authCodeResult.Value.IsCodeVerifierValid(request.CodeVerifier!))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Code verifier mismatch.");
            }

            Result<SvenToken> jwtResult = await _tokenProvider.CreateTokenAsync(authCodeResult.Value, cancellationToken);

            await _authCodeManager.ClearAuthCodeAsync(request.Code!, cancellationToken);

            return HandleTokenResult(jwtResult);
        }

        // RFC 6749 §4.4.2 — Access Token Request (Client Credentials Grant)
        private async Task<IActionResult> HandleClientCredentialsFlow(
            TokenRequest request, CancellationToken cancellationToken)
        {
            // Step 1: authenticate the client (RFC 6749 §2.3)
            // IClientAuthService is internal to Sven; resolved via RequestServices to avoid
            // public constructor accessibility violation.
            IClientAuthService clientAuthService = HttpContext.RequestServices.GetRequiredService<IClientAuthService>();
            Result<Client> clientResult = await clientAuthService.AuthenticateClientAsync(
                Request, cancellationToken);
            if (clientResult.IsError)
            {
                _logger.LogResultError(clientResult.Error);
                if (AuthConstants.OAuth.Errors.InvalidRequest.Equals(clientResult.Error.Code))
                {
                    return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest,
                        clientResult.Error.ErrorMessage);
                }
                // RFC 6749 §5.2: invalid_client MUST be 401 with WWW-Authenticate header
                Response.Headers["WWW-Authenticate"] = "Basic realm=\"Sven\"";
                return OAuthError(AuthConstants.OAuth.Errors.InvalidClient,
                    "Client authentication failed.", System.Net.HttpStatusCode.Unauthorized);
            }

            // Step 2: validate scope (RFC 6749 §4.4.2)
            string? effectiveScope = DetermineEffectiveScope(clientResult.Value, request.Scope);
            if (effectiveScope is null)
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidScope,
                    "Requested scope exceeds registered client scope.");
            }

            // Step 3: issue machine token (RFC 9068)
            Result<SvenToken> tokenResult = await _tokenProvider.CreateMachineTokenAsync(
                clientResult.Value, effectiveScope, cancellationToken);
            return HandleTokenResult(tokenResult);
        }

        // RFC 8693 — Token Exchange
        private async Task<IActionResult> HandleTokenExchangeFlow(
            TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SubjectToken))
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "subject_token is required.");
            if (string.IsNullOrWhiteSpace(request.Resource))
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "resource (provider) is required.");
            if (string.IsNullOrWhiteSpace(request.Scope))
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "scope (feature key) is required.");

            IClientAuthService clientAuthService =
                HttpContext.RequestServices.GetRequiredService<IClientAuthService>();
            Result<Client> clientAuthResult = await clientAuthService.AuthenticateClientAsync(Request, cancellationToken);
            if (clientAuthResult.IsError)
            {
                Response.Headers["WWW-Authenticate"] = "Basic realm=\"Sven\"";
                return OAuthError(AuthConstants.OAuth.Errors.InvalidClient, "Client authentication failed.",
                    System.Net.HttpStatusCode.Unauthorized);
            }

            ITokenExchangeService tokenExchangeService =
                HttpContext.RequestServices.GetRequiredService<ITokenExchangeService>();
            Result<TokenExchangeResponse> result = await tokenExchangeService.ExchangeAsync(
                clientAuthResult.Value.Identifier,
                request.SubjectToken!,
                request.Resource!,
                request.Scope!,
                cancellationToken);

            if (result.IsError)
                return OAuthError(result.Error.Code, result.Error.ErrorMessage);

            return Ok(result.Value);
        }

        private static string? DetermineEffectiveScope(Client client, string? requestedScope)
        {
            if (string.IsNullOrWhiteSpace(requestedScope))
            {
                return client.Scope.Scope;   // omitted → issue all registered scopes
            }
            if (!client.IsScopeGranted(requestedScope))
            {
                return null;   // exceeds registered → reject (invalid_scope)
            }
            return requestedScope;
        }

        private IActionResult HandleTokenResult(Result<SvenToken> result)
        {
            if (result.IsError)
            {
                _logger.LogResultError(result.Error);
                return result.Error.ToOAuthError();
            }
            return Ok(result.Value);
        }

        private static bool IsGrantType(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
