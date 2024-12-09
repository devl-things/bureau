using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.Identity.UI.Constants;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class SetPassword
    {
        private string? message;
        private BureauUser user = default!;
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

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
           
            if (userResult.IsSuccess)
            {
                user = userResult.Value;
                Result<bool> isPasswordSetResult = await _accountManager.IsPasswordSetAsync(user);
                if (isPasswordSetResult.IsSuccess && isPasswordSetResult.Value)
                {
                    _redirectManager.RedirectTo(BureauIdentityUIUris.ManageChangePassword);
                }
            }
            // TODO what if not
        }

        private async Task OnValidSubmitAsync()
        {
            var addPasswordResult = await _accountManager.SetPasswordAsync(user, Input.NewPassword!);
            if (addPasswordResult.IsError)
            {
                //TODO [StatusMessage]
                message = $"Error: {addPasswordResult.Error.ErrorMessage}";
                return;
            }

            Result refreshSignInResult = await _signInManager.RefreshSignInAsync(user);
            if (refreshSignInResult.IsError)
            {
                // TODO [StatusMessage] Show status in status message
                _redirectManager.RedirectToCurrentPageWithStatus("Error: ...", HttpContext);
                return;
            }
            //TODO [StatusMessage]
            _redirectManager.RedirectToCurrentPageWithStatus("Your password has been set.", HttpContext);
        }

        private sealed class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }
    }
}
