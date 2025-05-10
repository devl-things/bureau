using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.SignIn
{
    public class PlainSignInPageModel : SignInPageModel
    {
        public PlainSignInPageModel(ILogger<PlainSignInPageModel> logger, IUserClaimsProvider userProvider) : base(logger, userProvider)
        {
        }
        public override IActionResult HandleGetRequest()
        {
            return Page();
        }

        public override async Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default)
        {
            Result<ClaimsPrincipal> claimsPrincipalResult = await _userProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                ErrorMessage = AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword;
                return Page();
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);

            return Redirect(Endpoints.Account.AccountInfo);
        }
    }
}
