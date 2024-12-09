using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class ResetAuthenticator
    {
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;
        [Inject]
        private ILogger<ResetAuthenticator> _logger { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        private async Task OnSubmitAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
            if (userResult.IsSuccess)
            {
                BureauUser user = userResult.Value;
                //TODO [exception]
                await _accountManager.SetTwoFactorEnabledAsync(user, false);
                await _accountManager.ResetAuthenticatorKeyAsync(user);

                //TODO [StatusMessage]
                _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

                Result refreshSignInResult = await _signInManager.RefreshSignInAsync(user);
                if (refreshSignInResult.IsError)
                {
                    // TODO [StatusMessage] Show status in status message
                    _redirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
                    return;
                }

                //TODO [PageAddress]
                _redirectManager.RedirectToWithStatus(
                    "Account/Manage/EnableAuthenticator",
                    "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.",
                    HttpContext);
            }                  
        }
    }
}
