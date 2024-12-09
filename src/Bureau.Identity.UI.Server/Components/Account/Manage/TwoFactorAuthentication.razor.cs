using Bureau.Core;
using Bureau.Identity.Managers;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class TwoFactorAuthentication
    {
        private bool canTrack;
        private bool hasAuthenticator;
        private int recoveryCodesLeft;
        private bool is2faEnabled;
        private bool isMachineRemembered;

        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
            if (userResult.IsSuccess)
            {
                BureauUser user = userResult.Value;
                // TODO [ITrackingConsentFeature]
                canTrack = HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;
                Result<string?> hasAuthenticatorResult = await _accountManager.GetAuthenticatorKeyAsync(user);
                hasAuthenticator = hasAuthenticatorResult.IsSuccess && !string.IsNullOrWhiteSpace(hasAuthenticatorResult.Value);
                Result<bool> is2faEnabledResult = await _accountManager.IsTwoFactorEnabledAsync(user);
                is2faEnabled = is2faEnabledResult.IsSuccess && is2faEnabledResult.Value;

                Result<bool> isMachineRememberedResult = await _signInManager.IsTwoFactorClientRememberedAsync(user);
                isMachineRemembered = isMachineRememberedResult.IsSuccess && isMachineRememberedResult.Value;
                Result<int> recoveryCodesLeftResult = await _accountManager.CountRecoveryCodesAsync(user);
                recoveryCodesLeft = recoveryCodesLeftResult.IsSuccess? recoveryCodesLeftResult.Value : 0;
            }

        }

        private async Task OnSubmitForgetBrowserAsync()
        {
            await _signInManager.ForgetTwoFactorClientAsync();

            _redirectManager.RedirectToCurrentPageWithStatus(
                "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.",
                HttpContext);
        }
    }
}
