using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class DeletePersonalData
    {
        private string? message;
        private BureauUser user = default!;
        private bool requirePassword;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        private IUserManager _userManager { get; set; } = default!;
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;
        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;
        [Inject]
        private ILogger<DeletePersonalData> _logger { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Input ??= new();
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
            Result<bool> isPasswordSetResult = await _accountManager.IsPasswordSetAsync(user);
            if (userResult.IsSuccess && isPasswordSetResult.IsSuccess)
            {
                user = userResult.Value;
                requirePassword = isPasswordSetResult.Value;
            }
        }

        private async Task OnValidSubmitAsync()
        {
            if (requirePassword)
            {
                Result<bool> checkPasswordResult = await _accountManager.IsPasswordValidAsync(user, Input.Password);
                if (checkPasswordResult.IsSuccess && !checkPasswordResult.Value)
                {
                    message = "Error: Incorrect password.";
                    return;
                }
                //TODO else what if error
            }

            Result result = await _userManager.RemoveUserAsync(user);
            if (result.IsError)
            {
                //TODO [StatusMessage]
                throw new InvalidOperationException("Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            //TODO [StatusMessage]
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", user.Id);

            _redirectManager.RedirectToCurrentPage();
        }

        private sealed class InputModel
        {
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }
    }
}
