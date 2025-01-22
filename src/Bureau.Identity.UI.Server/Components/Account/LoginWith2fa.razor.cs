using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Bureau.Core;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class LoginWith2fa
    {
        [Inject]
        private ILogger<LoginWithRecoveryCode> _logger { get; set; } = default!;

        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        private string? _message;
        private BureauUser _user = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        [SupplyParameterFromQuery]
        private bool RememberMe { get; set; }

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
            string authenticatorCode = Input.TwoFactorCode!.Replace(" ", string.Empty).Replace("-", string.Empty);
            BureauSignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, RememberMe, Input.RememberMachine);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", _user.Id);
                _redirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", _user.Id);
                _redirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", _user.Id);
                _message = "Error: Invalid authenticator code.";
            }
        }

        private sealed class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string? TwoFactorCode { get; set; }

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }
    }
}
