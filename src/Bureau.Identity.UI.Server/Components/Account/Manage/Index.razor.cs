using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class Index
    {
        private BureauUser user = default!;
        private string? username;
        private string? phoneNumber;

        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
            if (userResult.IsSuccess)
            {
                user = userResult.Value;
                username = user.UserName;

                //TODO [error handling]
                Result<string?> getPhoneNumberResult = await _accountManager.GetPhoneNumberAsync(user);

                phoneNumber = getPhoneNumberResult.IsSuccess ? getPhoneNumberResult.Value : null;
                Input.PhoneNumber ??= phoneNumber;
            }
        }

        private async Task OnValidSubmitAsync()
        {
            if (Input.PhoneNumber != phoneNumber)
            {
                //
                Result setPhoneResult = await _accountManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (setPhoneResult.IsError)
                {
                    _redirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
                }
            }

            Result refreshSignInResult = await _signInManager.RefreshSignInAsync(user);
            if (refreshSignInResult.IsError)
            {
                // TODO [StatusMessage] Show status in status message
                _redirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
                return;
            }
            _redirectManager.RedirectToCurrentPageWithStatus("Your profile has been updated", HttpContext);
        }

        private sealed class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }
        }
    }
}
