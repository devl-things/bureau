using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;

namespace Sven.Controllers.Connect
{
    [ApiController]
    [Route(Endpoints.Connect.Authorize)]
    public class AuthorizeController : ConnectController
    {
        private readonly IClientProvider _clientProvider;
        private readonly AuthCodeProvider _authCodeManager;

        public AuthorizeController(ILogger<AuthorizeController> logger, IClientProvider clientProvider, AuthCodeProvider authCodeManager) : base(logger)
        {
            _clientProvider = clientProvider;
            _authCodeManager = authCodeManager;
        }
        [HttpGet]
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
            if (!UriValidator.IsRedirectUriValid(request.RedirectUri))
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

                OAuthRequest oauthRequest = new()
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
                return Redirect(Endpoints.Connect.SignInPkce);
            }
            return RedirectWithOAuthError(request.RedirectUri, AuthConstants.OAuth.Errors.UnsupportedResponseType, AuthConstants.OAuth.ErrorDescriptions.UnsupportedResponseType, request.State);
        }

        [HttpGet(Endpoints.Connect.ContinuePath)]
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

            Result<string> codeResult = await _authCodeManager.CreateAuthCodeAsync(requestResult.Value, [.. result.Principal.Claims], cancellationToken);

            if (codeResult.IsError)
            {
                _logger.LogResultError(codeResult.Error);
                return RedirectWithOAuthError(requestResult.Value.RedirectUri, AuthConstants.OAuth.Errors.ServerError, AuthConstants.OAuth.ErrorDescriptions.CreationAuthCodeFailed, requestResult.Value.State);
            }
            return RedirectWithOAuthCode(requestResult.Value.RedirectUri, codeResult.Value, requestResult.Value.State);
        }

    }
}
