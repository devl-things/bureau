using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public class PlainSignInPageModel : SignInPageModel
    {
        public PlainSignInPageModel(ILogger<PlainSignInPageModel> logger, ConnectTranslations translations, IUserClaimsProvider userClaimsProvider) :
            base(logger, translations, userClaimsProvider)
        {
        }
        public override Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IActionResult>(Page());
        }

        public override async Task<IActionResult> HandleLoginAsync(LoginCredentialsRequest credentials, CancellationToken cancellationToken = default)
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
