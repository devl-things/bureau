using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
    public partial class ConnectController : ControllerBase
    {
        private readonly ILogger<ConnectController> _logger;
        private readonly AuthCodeProvider _authCodeManager;
        private readonly IUserClaimsProvider _userProvider;
        private readonly IClientProvider _clientProvider;
        private readonly ITokenProvider _tokenProvider;

        public ConnectController(ILogger<ConnectController> logger, AuthCodeProvider authCodeManager, IStore<string, OAuthRequest> pkceRequestStore,
        IUserClaimsProvider userProvider, IClientProvider clientProvider, ITokenProvider tokenProvider)
        {
            _logger = logger;
            _authCodeManager = authCodeManager;
            _userProvider = userProvider;
            _clientProvider = clientProvider;
            _tokenProvider = tokenProvider;
        }

        [HttpGet(Endpoints.Connect.AuthorizePath)]
        [DisableAutoValidation]
        [ServiceFilter(typeof(OAuthValidationFilter))]
        public async Task<IActionResult> AuthorizeAsync([FromQuery] AuthorizeRequest request, CancellationToken cancellationToken = default)
        {
            Result<Client> clientResult = await _clientProvider.GetClientAsync(request.ClientId, cancellationToken);
            if (clientResult.IsError)
            {
                _logger.LogResultError(clientResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidRequest);
            }
            if (!(Uri.TryCreate(request.RedirectUri, UriKind.Absolute, out Uri? redirectUri) && string.IsNullOrWhiteSpace(redirectUri.Fragment)))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidRedirectUriFormat);
            }

            if (!clientResult.Value.IsValidRedirectUri(request.RedirectUri))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidRequest);
            }

            if (IsResponseType(request.ResponseType, AuthConstants.OAuth.ResponseTypes.Code))
            {
                if (string.IsNullOrWhiteSpace(request.CodeChallenge))
                {
                    return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.InvalidRequest,
                        "Code challenge not properly defined", request.State);
                }
                if (!clientResult.Value.IsScopeGranted(request.Scope))
                {
                    return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.InvalidScope,
                        AuthConstants.OAuth.ErrorDescriptions.RequestedScopeNotGranted, request.State);
                }

                OAuthRequest oauthRequest = new OAuthRequest
                {
                    ClientId = request.ClientId,
                    RedirectUri = request.RedirectUri,
                    Scope = request.Scope,
                    CodeChallenge = request.CodeChallenge,
                    CodeChallengeMethod = _authCodeManager.GetCodeChallengeMethod(request.CodeChallengeMethod),
                    State = request.State,
                    Nonce = request.Nonce,
                };

                Result<string> pkceKeyResult = await _authCodeManager.CreateOAuthRequestAsync(oauthRequest, cancellationToken);

                if (pkceKeyResult.IsError)
                {
                    _logger.LogResultError(pkceKeyResult.Error);
                    return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.ServerError,
                        AuthConstants.OAuth.ErrorDescriptions.CreationAuthCodeFailed, request.State);
                }

                Response.Cookies.Append(AuthConstants.CookieNames.PkceKey, pkceKeyResult.Value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                return Redirect(Endpoints.Connect.AuthorizePage);
            }
            return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.UnsupportedResponseType, AuthConstants.OAuth.ErrorDescriptions.UnsupportedResponseType, request.State);
        }

        private static bool IsResponseType(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost(Endpoints.Connect.AuthorizeLoginPath)]
        public async Task<IActionResult> LoginAsync([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken = default)
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, $"{AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState} ({AuthConstants.CookieNames.PkceKey}).");
            }

            if (!_authCodeManager.ExistsPkceKey(pkceKey!))
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
            Result<OAuthRequest> requestResult = await _authCodeManager.GetOAuthRequestAsync(pkceKey!, cancellationToken);
            if (requestResult.IsError)
            {
                _logger.LogResultError(requestResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidRequest, "Invalid or expired authorization state.");
            }
            Result removeResult = await _authCodeManager.ClearOAuthRequestAsync(pkceKey!, cancellationToken);
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
                return RedirectWithOAuthError(requestResult.Value.RedirectUri, AuthConstants.OAuth.Errors.ServerError, AuthConstants.OAuth.ErrorDescriptions.CreationAuthCodeFailed, requestResult.Value.State);
            }
            return RedirectWithOAuthCode(requestResult.Value.RedirectUri, codeResult.Value, requestResult.Value.State);
        }

        private IActionResult OAuthError(string error, string? description = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return StatusCode((int)statusCode, new OAuthError(error, description));
        }
        private IActionResult OAuthError(ResultError resultError)
        {
            return BadRequest(new OAuthError(resultError.ErrorMessage, resultError.LogMessage));
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
    }
}