using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Bureau.Core;
using Bureau.Identity.Managers;
using Bureau.Identity.Models;
using Bureau.UI.Managers;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ForgotPassword
    {
        [Inject]
        IUserManager _userManager { get; set; } = default!;

        [Inject]
        IAccountManager _accountManager {  get; set; } = default!;

        [Inject]
        IEmailManager _emailManager { get; set; } = default!;


        //TODO [NavigationManager] change to Bureau
        [Inject]
        NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        BureauRedirectManager _redirectManager { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        private async Task OnValidSubmitAsync()
        {
            Result<IUserId> userResult = await _userManager.GetUserIdByEmailAsync(Input.Email);
            Result<bool> isEmailConfirmedResult = await _accountManager.IsEmailConfirmedAsync(userResult.Value);
            if (userResult.IsError || isEmailConfirmedResult.IsError || !isEmailConfirmedResult.Value)
            {
                // Don't reveal that the user does not exist or is not confirmed
                _redirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            Result<string> codeResult = await _accountManager.GeneratePasswordResetTokenAsync(userResult.Value);
            //TODO encoding/decoding
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));
            //TODO [NavigationManager]
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ResetPassword").AbsoluteUri,
                new Dictionary<string, object?> { ["code"] = code });

            await _emailManager.SendPasswordResetLinkAsync(userResult.Value, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            _redirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
        }

        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";
        }
    }
}
