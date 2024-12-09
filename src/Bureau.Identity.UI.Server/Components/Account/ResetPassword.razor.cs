using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Bureau.Identity.UI.Constants;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ResetPassword
    {
        [Inject]
        public IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        internal BureauRedirectManager RedirectManager { get; init; }

        //TODO [SupplyParameterFromForm]
        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? Code { get; set; }

        private string? Message { get; set; }

        protected override void OnInitialized()
        {
            //TODO [PageAddress]
            if (Code is null)
            {
                RedirectManager.RedirectTo(BureauIdentityUIUris.InvalidPasswordReset);
            }

            Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        }

        private async Task OnValidSubmitAsync()
        {           
            Result result = await _accountManager.ResetPasswordAsync(Input.Email, Input.Code, Input.Password);
            if (result.IsSuccess)
            {
                RedirectManager.RedirectTo(BureauIdentityUIUris.ResetPasswordConfirmation);
            }
            //TODO [StatusMessage] put all errors as 'Some error' but then in log put all info
            Message = result.Error.ErrorMessage;
        }

        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";

            [Required]
            public string Code { get; set; } = "";
        }
    }
}
