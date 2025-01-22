using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.Identity.UI.Constants;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class Email
    {
        //@inject UserManager<ApplicationUser> UserManager
        //@inject IEmailSender<ApplicationUser> EmailSender

        [Inject]
        private IEmailManager _emailManager { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauNavigationManager _navigationManager { get; set; }

        private string? message;
        private BureauUser user = default!;
        private string? email;
        private bool isEmailConfirmed;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm(FormName = "change-email")]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> result = await _identityProvider.GetUserAsync();
            if (result.IsSuccess) 
            {
                user = result.Value;
                email = user.Email;
                Result<bool> isEmailConfirmedResult = await _accountManager.IsEmailConfirmedAsync(user);
                //TODO what here if is not good
                isEmailConfirmed = isEmailConfirmedResult.IsSuccess? isEmailConfirmedResult.Value : false;

                Input.NewEmail ??= email;
            }
            //TODO else what

        }

        private async Task OnValidSubmitAsync()
        {
            if (Input.NewEmail is null || Input.NewEmail == email)
            {
                message = "Your email is unchanged.";
                return;
            }

            Result<string> codeResult = await _accountManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));
            //TODO [PageAddress]
            var callbackUrl = _navigationManager.NavigationManager.GetUriWithQueryParameters(
            _navigationManager.NavigationManager.ToAbsoluteUri(BureauIdentityUIUris.ConfirmEmailChange).AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = user.Id, ["email"] = Input.NewEmail, ["code"] = code });

            await _emailManager.SendConfirmationLinkAsync(user, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Confirmation link to change email sent. Please check your email.";
        }

        private async Task OnSendEmailVerificationAsync()
        {
            if (email is null)
            {
                return;
            }
            Result<string> codeResult = await _accountManager.GenerateEmailConfirmationTokenAsync(user);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));
            // [PageAddress]
            var callbackUrl = _navigationManager.NavigationManager.GetUriWithQueryParameters(
            _navigationManager.NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = user.Id, ["code"] = code });

            await _emailManager.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Verification email sent. Please check your email.";
        }

        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string? NewEmail { get; set; }
        }
    }
}
