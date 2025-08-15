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
        //TODO new endpoint for registered users

        //TODO new endpoint for plain type, which will sign 
        [HttpGet(Modes.ExternalLogin.Plain)]
        public async Task<IActionResult> AuthorizePlainAsync(CancellationToken cancellationToken = default)
        {
            //TODO take the principal, find out who is the user
            //if user is new, create user and bring user to the page where s/he will fill out whatever else is needed
            //if is not new, sign in user and redirect to url

            AuthenticateResult result = await HttpContext.AuthenticateAsync(AuthConstants.AuthenticationSchemes.External);

            if (!result.Succeeded || result.Principal == null || result.Principal.Claims == null)
            {
                // Handle external authentication failure
                return RedirectWithOAuthError("/connect/error", AuthConstants.OAuth.Errors.AccessDenied, "User authentication failed.", null);
            }

            string returnUrl = Endpoints.Account.AccountInfo; // default return URL

            if ((result.Properties?.Items.TryGetValue(AuthConstants.PropertyNames.RedirectUrl, out string? url) ?? false) &&
                !string.IsNullOrWhiteSpace(url))
            {
                returnUrl = url!;
            }


            //ClaimsPrincipal externalPrincipal = result.Principal;
            //string? externalProvider = result.Properties?.Items["scheme"] ?? externalPrincipal.Identity?.AuthenticationType;
            //string? externalUserId = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrWhiteSpace(externalProvider) || string.IsNullOrWhiteSpace(externalUserId))
            //{
            //    return RedirectWithOAuthError("/connect/error", AuthConstants.OAuth.Errors.AccessDenied, "Invalid external login information.", null);
            //}

            //// Example user lookup
            //Result<User> userResult = await _userProvider.FindByExternalProviderAsync(externalProvider, externalUserId, cancellationToken);

            //if (userResult.IsError)
            //{
            //    _logger.LogResultError(userResult.Error);

            //    // New user flow: store external identity info in TempData or a temporary cookie, then redirect
            //    ExternalLoginTempModel tempModel = new ExternalLoginTempModel
            //    {
            //        Provider = externalProvider,
            //        ProviderUserId = externalUserId,
            //        Email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value
            //    };

            //    TempData.Set(AuthConstants.TempDataKeys.ExternalLoginTemp, tempModel);

            //    await HttpContext.SignOutAsync(AuthConstants.AuthenticationSchemes.External);

            //    return Redirect("/connect/complete-profile");
            //}

            //// Existing user flow: create app principal and sign in
            //ClaimsPrincipal appPrincipal = _principalFactory.Create(userResult.Value);

            //await HttpContext.SignInAsync(AuthConstants.AuthenticationSchemes., appPrincipal);

            //await HttpContext.SignOutAsync(AuthConstants.AuthenticationSchemes.External);

            return Redirect(returnUrl);
        }

        [HttpGet(Modes.ExternalLogin.Pkce)]
        public async Task<IActionResult> AuthorizePkceAsync(CancellationToken cancellationToken = default)
        {
            // TODO this should be only for pkce
            // TODO Sign out the user that is automatically signed in by framework comming from external provider
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
