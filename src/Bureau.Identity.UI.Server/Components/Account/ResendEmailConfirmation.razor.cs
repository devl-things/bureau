using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ResendEmailConfirmation
    {
        [Inject]
        public IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        public required IEmailManager _emailManager { get; init; }
        [Inject]
        public IUserManager _userManager { get; set; } = default!;
        [Inject]
        public required NavigationManager NavigationManager { get; init; }
        [Inject]
        internal BureauRedirectManager RedirectManager { get; init; }

        private string? message;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        private async Task OnValidSubmitAsync()
        {
            Result<IUserId> userResult = await _userManager.GetUserIdByEmailAsync(Input.Email!);
            if (userResult.IsError)
            {
                // TODO [StatusMessage]
                message = "Verification email sent. Please check your email.";
                return;
            }

            Result<string> codeResult = await _accountManager.GenerateEmailConfirmationTokenAsync(userResult.Value);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));
            //TODO [PageAddress]
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userResult.Value.Id, ["code"] = code });
            await _emailManager.SendConfirmationLinkAsync(userResult.Value, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Verification email sent. Please check your email.";
        }

        private sealed class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";
        }
    }
}
