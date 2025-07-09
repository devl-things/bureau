using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public class PkceSignInPageModel : SignInPageModel
    {
        private readonly AuthCodeProvider _authCodeManager;

        public PkceSignInPageModel(ILogger<PkceSignInPageModel> logger, ConnectTranslations translations, IUserClaimsProvider userClaimsProvider, AuthCodeProvider authCodeManager)
            : base(logger, translations, userClaimsProvider)
        {
            Mode = PageModelTypes.SignIn.Pkce;
            _authCodeManager = authCodeManager;
        }
        public override Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default)
        {
            if (ValidatePkceKey() is { IsError: true } result)
            {
                return Task.FromResult(OAuthError(result.Error));
            }

            return Task.FromResult<IActionResult>(Page());
        }

        public override async Task<IActionResult> HandleLoginAsync(LoginCredentialsRequest credentials, CancellationToken cancellationToken = default)
        {
            if (ValidatePkceKey() is { IsError: true } result)
            {
                return OAuthError(result.Error);
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                ErrorMessage = AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword;
                return Page();
            }

            await SignInUserAsync(claimsPrincipalResult.Value, cancellationToken);

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
            return BadRequest(new OAuthError(resultError.ErrorMessage, resultError.UserMessage));
        }
    }
}
