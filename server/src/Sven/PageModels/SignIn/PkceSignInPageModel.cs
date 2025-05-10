using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.SignIn
{
    public class PkceSignInPageModel : SignInPageModel
    {
        private readonly AuthCodeProvider _authCodeManager;

        public PkceSignInPageModel(ILogger<PkceSignInPageModel> logger, IUserClaimsProvider userProvider, AuthCodeProvider authCodeManager)
            : base(logger, userProvider)
        {
            Mode = PageModelTypes.SignIn.Pkce;
            _authCodeManager = authCodeManager;
        }
        public override IActionResult HandleGetRequest()
        {
            if (ValidatePkceKey() is { IsError: true } result)
            {
                return OAuthError(result.Error);
            }

            return Page();
        }

        public override async Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default)
        {
            if (ValidatePkceKey() is { IsError: true } result)
            {
                return OAuthError(result.Error);
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                ErrorMessage = AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword;
                return Page();
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);

            return Redirect(Endpoints.Connect.AuthorizeContinue);
        }

        private Result ValidatePkceKey()
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return new ResultError(AuthConstants.OAuth.Errors.InvalidRequest, $"{AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState} ({AuthConstants.CookieNames.PkceKey}).");
            }

            if (!_authCodeManager.ExistsPkceKey(pkceKey!))
            {
                return new ResultError(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidAuthorizationState);
            }
            return true;
        }

        protected IActionResult OAuthError(ResultError resultError)
        {
            return BadRequest(new OAuthError(resultError.ErrorMessage, resultError.LogMessage));
        }
    }
}
