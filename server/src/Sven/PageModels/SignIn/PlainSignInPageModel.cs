using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.SignIn
{
    public class PlainSignInPageModel : SignInPageModel
    {
        public PlainSignInPageModel(ILogger<PlainSignInPageModel> logger, IUserClaimsProvider userClaimsProvider) : base(logger, userClaimsProvider)
        {
        }
        public override Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IActionResult>(Page());
        }

        public override async Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default)
        {
            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(credentials.Username, credentials.Password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                ErrorMessage = AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword;
                return Page();
            }
            await SignInUserAsync(claimsPrincipalResult.Value, cancellationToken);

            return Redirect(Endpoints.Account.AccountInfo);
        }
    }
}
