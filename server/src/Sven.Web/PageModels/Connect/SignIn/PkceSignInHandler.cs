using Bureau;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public class PkceSignInHandler : ISignInGetHandler, ISignInPostHandler
    {
        private readonly ILogger<PkceSignInHandler> _logger;
        private readonly IUserClaimsProvider _userClaimsProvider;
        private readonly IAuthCodeService _authCodeService;
        private readonly ErrorTranslations _errorTranslations;

        public PkceSignInHandler(ILogger<PkceSignInHandler> logger, IUserClaimsProvider userClaimsProvider, IAuthCodeService authCodeService, ErrorTranslations errorTranslations)
        {
            _logger = logger;
            _userClaimsProvider = userClaimsProvider;
            _authCodeService = authCodeService;
            _errorTranslations = errorTranslations;
        }

        public Task<IActionResult> HandleGetAsync(SignInContext context, CancellationToken cancellationToken = default)
        {
            if (ValidatePkceKey(context) is { IsError: true } result)
            {
                return Task.FromResult(result.Error.ToOAuthError());
            }

            return Task.FromResult<IActionResult>(context.Page());
        }

        public async Task<IActionResult> HandlePostAsync(SignInContext context, CancellationToken cancellationToken = default)
        {
            if (ValidatePkceKey(context) is { IsError: true } result)
            {
                return result.Error.ToOAuthError();
            }

            LoginCredentialsRequest credentials = context.Credentials!;
            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                context.ErrorMessage = _errorTranslations.InvalidUsernamePassword;
                return context.Page();
            }

            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);
            return context.Redirect(Endpoints.Connect.AuthorizePkce);
        }

        private Result ValidatePkceKey(SignInContext context)
        {
            if (!context.Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidRequest, $"{AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState} ({AuthConstants.CookieNames.PkceKey}).");
            }

            if (!_authCodeService.ExistsPkceKey(pkceKey!))
            {
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidRequest, AuthConstants.OAuth.ErrorDescriptions.InvalidAuthorizationState);
            }

            return true;
        }
    }
}
