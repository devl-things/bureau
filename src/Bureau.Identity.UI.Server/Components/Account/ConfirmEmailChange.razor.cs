using Bureau.Core;
using Bureau.Identity.Constants;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.Identity.UI.Constants;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Bureau.Identity.Factories;
using Bureau.Core.Factories;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ConfirmEmailChange
    {
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        private string? message;

        [CascadingParameter]
        private HttpContext _httpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? _userId { get; set; }

        [SupplyParameterFromQuery]
        private string? _email { get; set; }

        [SupplyParameterFromQuery]
        private string? Code { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(_userId) ||
                string.IsNullOrWhiteSpace(_email) ||
                string.IsNullOrWhiteSpace(Code)) { }
            {
                _redirectManager.RedirectToWithStatus(BureauIdentityUIUris.Login, BureauIdentityMessages.EmailChange, _httpContext);
            }

            IUserId userId = BureauUserIdFactory.CreateIBureauUserId(_userId);

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            Result changeEmailResult = await _accountManager.ChangeEmailAsync(userId, _email, code);
            if(changeEmailResult.IsError) 
            {
                // TODO [StatusMessage] Show status in status message
                message = changeEmailResult.Error.ErrorMessage;
                return;
            }

            // In our UI email and user name are one and the same, so when we update the email
            // we need to update the user name.
            // Not true in our case
            //var setUserNameResult = await UserManager.SetUserNameAsync(user, _email);
            //if (!setUserNameResult.Succeeded)
            //{
            //    message = "Error changing user name.";
            //    return;
            //}

            Result refreshSignInResult = await _signInManager.RefreshSignInAsync(userId);
            if (refreshSignInResult.IsError)
            {
                // TODO [StatusMessage] Show status in status message
                message = refreshSignInResult.Error.ErrorMessage;
                return;
            }

            message = BureauIdentityUIMessages.ConfirmEmailChange;
        }
    }
}
