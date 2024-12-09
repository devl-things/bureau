using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class EnableAuthenticator
    {
        [Inject]
        private ILogger<EnableAuthenticator> _logger { get; set; } = default!;
        [Inject]
        private UrlEncoder _urlEncoder { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        //@inject UserManager<ApplicationUser> UserManager
        //@inject BureauIdentityManager UserAccessor

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        private string? message;
        private BureauUser user = default!;
        private string? sharedKey;
        private string? authenticatorUri;
        private IEnumerable<string>? recoveryCodes;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> result = await _identityProvider.GetUserAsync();
            if (result.IsSuccess)
            {
                await LoadSharedKeyAndQrCodeUriAsync(result.Value);
            }
            //TODO else what
        }

        private async Task OnValidSubmitAsync()
        {
            // Strip spaces and hyphens
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            Result<bool> is2faTokenValidResult = await _accountManager.IsTwoFactorTokenValidAsync(user, verificationCode);

            if (is2faTokenValidResult.IsError || !is2faTokenValidResult.Value)
            {
                message = "Error: Verification code is invalid.";
                return;
            }

            await _accountManager.SetTwoFactorEnabledAsync(user, true);

            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", user.Id);
            message = "Your authenticator app has been verified.";

            Result<int> countRecoveryCodesResult = await _accountManager.CountRecoveryCodesAsync(user);
            if (countRecoveryCodesResult.IsSuccess && countRecoveryCodesResult.Value == 0)
            {
                recoveryCodes = (await _accountManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).Value;
            }
            else
            {
                _redirectManager.RedirectToWithStatus("Account/Manage/TwoFactorAuthentication", message, HttpContext);
            }
        }

        private async ValueTask LoadSharedKeyAndQrCodeUriAsync(BureauUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            Result<string?> unformattedKeyResult = await _accountManager.GetAuthenticatorKeyAsync(user);
            if (unformattedKeyResult.IsSuccess && string.IsNullOrEmpty(unformattedKeyResult.Value))
            {
                await _accountManager.ResetAuthenticatorKeyAsync(user);
                unformattedKeyResult = await _accountManager.GetAuthenticatorKeyAsync(user);
            }

            sharedKey = FormatKey(unformattedKeyResult.Value!);

            var email = user.Email;
            authenticatorUri = GenerateQrCodeUri(email!, unformattedKeyResult.Value!);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                AuthenticatorUriFormat,
                _urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private sealed class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; } = "";
        }
    }
}
