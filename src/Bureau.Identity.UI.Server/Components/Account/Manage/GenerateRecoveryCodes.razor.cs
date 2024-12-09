using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class GenerateRecoveryCodes
    {
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;
        [Inject]
        private ILogger<GenerateRecoveryCodes> _logger { get; set; } = default!;

        private string? message;
        private BureauUser user = default!;
        private IEnumerable<string>? recoveryCodes;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
            if (userResult.IsSuccess)
            {
                Result<bool> is2factorEnabledResult = await _accountManager.IsTwoFactorEnabledAsync(userResult.Value);
                if (is2factorEnabledResult.IsSuccess && !is2factorEnabledResult.Value) 
                {
                    //TODO [StatusMessage]
                    throw new InvalidOperationException("Cannot generate recovery codes for user because they do not have 2FA enabled.");
                }
            }
            //TODO else what
        }

        private async Task OnSubmitAsync()
        {
            Result<List<string>?> generateNewResult = await _accountManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            recoveryCodes = generateNewResult.Value;
            message = "You have generated new recovery codes.";

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", user.Id);
        }
    }
}
