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
    public class PlainSignInHandler : ISignInGetHandler, ISignInPostHandler
    {
        private readonly ILogger<PlainSignInHandler> _logger;
        private readonly IUserClaimsProvider _userClaimsProvider;
        private readonly ErrorTranslations _errorTranslations;

        public PlainSignInHandler(ILogger<PlainSignInHandler> logger, IUserClaimsProvider userClaimsProvider, ErrorTranslations errorTranslations)
        {
            _logger = logger;
            _userClaimsProvider = userClaimsProvider;
            _errorTranslations = errorTranslations;
        }

        public Task<IActionResult> HandleGetAsync(SignInContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IActionResult>(context.Page());
        }

        public async Task<IActionResult> HandlePostAsync(SignInContext context, CancellationToken cancellationToken = default)
        {
            LoginCredentialsRequest credentials = context.Credentials!;
            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                context.ErrorMessage = _errorTranslations.InvalidUsernamePassword;
                return context.Page();
            }

            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);
            return context.Redirect(Endpoints.Account.AccountInfo);
        }
    }
}
