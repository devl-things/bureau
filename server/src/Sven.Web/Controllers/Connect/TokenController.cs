using Bureau;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
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

        public TokenController(ILogger<TokenController> logger, ITokenProvider tokenProvider, IAuthCodeService authCodeManager) : base(logger)
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
