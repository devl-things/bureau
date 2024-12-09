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
    public partial class Disable2fa
    {
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;
        [Inject]
        private ILogger<DeletePersonalData> _logger { get; set; } = default!;

        private BureauUser user = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> result = await _identityProvider.GetUserAsync();
            if (result.IsSuccess)
            {
                Result<bool> twoFactorEnabledResult = await _accountManager.IsTwoFactorEnabledAsync(user);
                if (HttpMethods.IsGet(HttpContext.Request.Method) && !twoFactorEnabledResult.Value)
                {
                    throw new InvalidOperationException("Cannot disable 2FA for user as it's not currently enabled.");
                }
            }
        }

        private async Task OnSubmitAsync()
        {
            Result disable2faResult = await _accountManager.SetTwoFactorEnabledAsync(user, false);
            if (disable2faResult.IsError)
            {
                //TODO [StatusMessage]
                throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
            }

            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", user.Id);
            //TODO [PageAddress]
            _redirectManager.RedirectToWithStatus(
                "Account/Manage/TwoFactorAuthentication",
                "2fa has been disabled. You can reenable 2fa when you setup an authenticator app",
                HttpContext);
        }
    }
}
