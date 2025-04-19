using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Abstractions.Services;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.Connect.Base)]
    public class ConnectController : ControllerBase
    {
        private readonly ILogger<ConnectController> _logger;
        private readonly AuthCodeProvider _authCodeManager;
        private readonly IStore<string, OAuthRequest> _pkceRequestStore;
        private readonly IUserClaimsProvider _userProvider;
        private readonly IClientProvider _clientProvider;
        private readonly ITokenProvider _tokenProvider;

        public ConnectController(ILogger<ConnectController> logger, AuthCodeProvider authCodeManager, IStore<string, OAuthRequest> pkceRequestStore,
        IUserClaimsProvider userProvider, IClientProvider clientProvider, ITokenProvider tokenProvider)
        {
            _logger = logger;
            _authCodeManager = authCodeManager;
            _pkceRequestStore = pkceRequestStore;
            _userProvider = userProvider;
            _clientProvider = clientProvider;
            _tokenProvider = tokenProvider;
        }

        [HttpGet(Endpoints.Connect.AuthorizePath)]
        [DisableAutoValidation]
        [ServiceFilter(typeof(OAuthValidationFilter))]
        public async Task<IActionResult> AuthorizeAsync([FromQuery] AuthorizeRequest request, CancellationToken cancellationToken = default)
        {
            if (!Uri.IsWellFormedUriString(request.RedirectUri, UriKind.Absolute))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidRedirectUriFormat);
            }
            Result<bool> isClientValidResult = await _clientProvider.IsValidAsync(request.ClientId, request.RedirectUri, cancellationToken);
            if (isClientValidResult.IsError || !isClientValidResult.Value)
            {
                _logger.LogResultError(isClientValidResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "Invalid request.");
            }

            if (!AuthConstants.OAuth.ResponseTypes.Code.Equals(request.ResponseType, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.UnsupportedResponseType,
                    AuthConstants.OAuth.ErrorDescriptions.UnsupportedResponseType, request.State);
            }

            if (!_authCodeManager.IsCodeChallengeValid(request.CodeChallenge, request.CodeChallengeMethod))
            {
                return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.InvalidRequest,
                    "Code challenge not properly defined", request.State);
            }


            Result<bool> isScopeValidResult = await _clientProvider.IsScopeValidAsync(request.ClientId, request.Scope, cancellationToken);

            if (isScopeValidResult.IsError || !isScopeValidResult.Value)
            {
                return RedirectWithOAuthError(request.RedirectUri, isScopeValidResult.Error, request.State);
            }

            OAuthRequest oauthRequest = new OAuthRequest
            {
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                Scope = request.Scope,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                State = request.State,
                Nonce = request.Nonce,
            };

            string pkceKey = Guid.NewGuid().ToString("N");
            Result storeResult = await _pkceRequestStore.StoreAsync(pkceKey, oauthRequest, cancellationToken);
            if (storeResult.IsError)
            {
                _logger.LogResultError(storeResult.Error);
                return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.ServerError,
                    "Authorization state couldn't be created", request.State);
            }

            Response.Cookies.Append(AuthConstants.CookieNames.PkceKey, pkceKey, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return Redirect(Endpoints.Connect.AuthorizePage);
        }

        [HttpPost(Endpoints.Connect.AuthorizeLoginPath)]
        public async Task<IActionResult> LoginAsync([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken = default)
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, $"{AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState} ({AuthConstants.CookieNames.PkceKey}).");
            }

            if (!_pkceRequestStore.Exists(pkceKey!))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "Invalid or expired authorization request.");
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userProvider.GetClaimsPrincipalAsync(username, password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.AccessDenied, "Invalid username or password.", HttpStatusCode.Unauthorized);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);

            return Redirect(Endpoints.Connect.AuthorizeContinue);
        }

        [HttpGet(Endpoints.Connect.AuthorizeContinuePath)]
        public async Task<IActionResult> CompleteAuthorizeAsync(CancellationToken cancellationToken = default)
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState);
            }
            Result<OAuthRequest> requestResult = await _pkceRequestStore.GetAsync(pkceKey!, cancellationToken);
            if (requestResult.IsError)
            {
                _logger.LogResultError(requestResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "Invalid or expired authorization state.");
            }
            Result removeResult = await _pkceRequestStore.RemoveAsync(pkceKey!, cancellationToken);
            if (removeResult.IsError)
            {
                _logger.LogResultError(removeResult.Error);
            }

            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null || result.Principal.Claims == null)
            {
                return RedirectWithOAuthError(requestResult.Value.RedirectUri, AuthConstants.OAuth.Errors.AccessDenied,
                    "User authentication failed.", requestResult.Value.State);
            }

            Result<string> codeResult = await _authCodeManager.CreateAuthCodeAsync(requestResult.Value, result.Principal.Claims.ToList(), cancellationToken);

            if (codeResult.IsError)
            {
                _logger.LogResultError(codeResult.Error);
                return RedirectWithOAuthError(requestResult.Value.RedirectUri, codeResult.Error, requestResult.Value.State);
            }
            return RedirectWithOAuthCode(requestResult.Value.RedirectUri, codeResult.Value, requestResult.Value.State);
        }

        [HttpPost(Endpoints.Connect.TokenPath)]
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
            Result<bool> isValidResult = await _tokenProvider.IsRefreshTokenValidAsync(request.RefreshToken!, request.ClientId, request.Scope, cancellationToken);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return OAuthError(isValidResult.Error);
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

            Result<AuthCode> authCodeResult = await _authCodeManager.GetAsync(request.Code!, cancellationToken);
            if (authCodeResult.IsError)
            {
                _logger.LogResultError(authCodeResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Invalid or expired authorization code.");
            }

            Result<bool> isValidResult = _authCodeManager.IsAuthCodeValid(authCodeResult.Value, request.ClientId, request.CodeVerifier!);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidGrant, "Code verifier mismatch or invalid client.");
            }

            Result<SvenToken> jwtResult = await _tokenProvider.CreateTokenAsync(authCodeResult.Value, cancellationToken);

            await _authCodeManager.ClearAsync(request.Code!, cancellationToken);

            return HandleTokenResult(jwtResult);
        }

        private IActionResult HandleTokenResult(Result<SvenToken> result)
        {
            if (result.IsError)
            {
                _logger.LogResultError(result.Error);
                return OAuthError(result.Error);
            }
            return Ok(result.Value);
        }

        private IActionResult OAuthError(string error, string? description = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return StatusCode((int)statusCode, new OAuthError(error, description));
        }
        private IActionResult OAuthError(ResultError resultError)
        {
            return BadRequest(new OAuthError(resultError.ErrorMessage, resultError.LogMessage));
        }
        private IActionResult RedirectWithOAuthError(string redirectUri, ResultError resultError, string? state)
        {
            return RedirectWithOAuthError(redirectUri, resultError.ErrorMessage, resultError.LogMessage, state);
        }
        private IActionResult RedirectWithOAuthError(string redirectUri, string error, string? errorDescription, string? state)
        {
            StringBuilder sb = new StringBuilder(redirectUri);
            sb.Append("?");
            sb = RedirectUrlWithOAuthError(sb, error, errorDescription);
            sb = RedirectUrlAppendState(sb, state);
            return Redirect(HttpUtility.UrlEncode(sb.ToString()));
        }

        private IActionResult RedirectWithOAuthCode(string redirectUri, string code, string? state)
        {
            StringBuilder sb = new StringBuilder(redirectUri);
            sb.Append("?").Append(AuthConstants.OAuth.FieldNames.Code).Append("=").Append(code);
            sb = RedirectUrlAppendState(sb, state);
            return Redirect(sb.ToString());
        }

        private static StringBuilder RedirectUrlWithOAuthError(StringBuilder redirectUrl, string error, string? errorDescription)
        {
            redirectUrl.Append(AuthConstants.OAuth.FieldNames.Error).Append("=").Append(error);
            if (!string.IsNullOrWhiteSpace(errorDescription))
            {
                redirectUrl.Append("&").Append(AuthConstants.OAuth.FieldNames.ErrorDescription)
                    .Append("=").Append(errorDescription);
            }
            return redirectUrl;
        }
        private static StringBuilder RedirectUrlAppendState(StringBuilder redirectUrl, string? state)
        {
            if (!string.IsNullOrWhiteSpace(state))
            {
                redirectUrl.Append("&").Append(AuthConstants.OAuth.FieldNames.State)
                    .Append("=").Append(state);
            }

            return redirectUrl;
        }

        private static bool IsGrantType(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}