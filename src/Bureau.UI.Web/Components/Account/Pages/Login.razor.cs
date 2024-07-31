using Bureau.UI.Web.Components.Account.Managers;
using Bureau.UI.Web.Components.Account.PageAddresses;
using Bureau.UI.Web.Components.Helpers;
using Bureau.UI.Web.Components.Shared;
using Bureau.UI.Web.Data;
using Bureau.UI.Web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class Login
    {

        private StatusMessageInput _statusMessage = new StatusMessageInput();

        [CascadingParameter]
        private HttpContext _httpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel _input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? _returnUrl { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Inject]
        private SignInManager<ApplicationUser> _signInManager { get; init; }
        [Inject]
        private ILogger<Login> _logger { get; init; }
        [Inject]
        private IdentityRedirectManager _redirectManager { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


        protected override async Task OnInitializedAsync()
        {
            if (HttpMethods.IsGet(_httpContext.Request.Method))
            {
                // Clear the existing external cookie to ensure a clean login process
                await _httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
        }

        public async Task LoginUser()
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(_input.UserName, _input.Password, _input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.Info(LogMessages.UserLogin, _input.UserName);
                _redirectManager?.RedirectTo(_returnUrl);
            }
            else if (result.RequiresTwoFactor)
            {
                _redirectManager?.RedirectTo(new LoginWith2faPageAddress(_returnUrl!, _input.RememberMe));
            }
            else if (result.IsLockedOut)
            {
                _logger.Warning(LogMessages.UserLockedOut, _input.UserName);
                _redirectManager?.RedirectTo(UriPages.Lockout);
            }
            else
            {
                _statusMessage.SetError(UIMessages.InvalidLogin);
            }
        }

        private sealed class InputModel
        {
            [Required]
            public string UserName { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}
