using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.Identity.UI.Constants;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class ChangePassword
    {
        private string? message;
        private BureauUser user = default!;
        private bool hasPassword;
        [Inject]
        private ILogger<ChangePassword> _logger { get; set; }
        [Inject]
        private IIdentityProvider _identityProvider { get; set; }
        [Inject]
        private IAccountManager _accountManager { get; set; }
        [Inject]
        private ISignInManager _signInManager { get; set; }
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; }

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
                if (isPasswordSetResult.IsSuccess && !isPasswordSetResult.Value)
                {
                    _redirectManager.RedirectTo(BureauIdentityUIUris.ManageSetPassword);
                }
            }
        }

        private async Task OnValidSubmitAsync()
        {
            var changePasswordResult = await _accountManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (changePasswordResult.IsError)
            {
                // TODO [StatusMessage]
                message = $"Error: {changePasswordResult.Error.ErrorMessage}";
                return;
            }

            Result refreshSignInResult = await _signInManager.RefreshSignInAsync(user);
            if (refreshSignInResult.IsError)
            {
                // TODO [StatusMessage] Show status in status message
                message = refreshSignInResult.Error.ErrorMessage;
                return;
            }
            //TODO [Status messages]
            _logger.LogInformation("User changed their password successfully.");

            _redirectManager.RedirectToCurrentPageWithStatus("Your password has been changed", HttpContext);
        }

        private sealed class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}
