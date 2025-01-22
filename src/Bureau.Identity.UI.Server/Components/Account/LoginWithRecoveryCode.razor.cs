using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Bureau.Core;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class LoginWithRecoveryCode
    {
        [Inject]
        private ILogger<LoginWithRecoveryCode> _logger { get; set; } = default!;

        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        private string? message;
        private BureauUser _user = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Ensure the user has gone through the username & password screen first
            Result<BureauUser> userResult = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (userResult.IsError)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }
            _user = userResult.Value;
        }

        private async Task OnValidSubmitAsync()
        {
            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);
            BureauSignInResult result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.IsSuccess)
            {
                //TODO [StatusMessage]
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", _user.Id);
                _redirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                _redirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", _user.Id);
                message = "Error: Invalid recovery code entered.";
            }
        }

        private sealed class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; } = "";
        }
    }
}
