using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.UI.Components;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class RegisterConfirmation
    {
        private string? emailConfirmationLink;

        private StatusMessageInput StatusMessage = new StatusMessageInput();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        //TODO [SupplyParameterFromQuery] names
        [SupplyParameterFromQuery]
        private string? UserName { get; set; }

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }
        [Inject]
        public IAccountManager _accountManager { get; set; } = default!;
        [Inject]
        public IUserManager _userManager { get; set; } = default!;
        [Inject]
        public required IEmailManager _emailManager { get; init; }
        [Inject]
        public required NavigationManager NavigationManager { get; init; }
        [Inject]
        internal BureauRedirectManager RedirectManager { get; init; }

        protected override async Task OnInitializedAsync()
        {
            if (UserName is null)
            {
                RedirectManager.RedirectTo("");
            }

            Result<IUserId> userResult = await _userManager.GetUserIdByNameAsync(UserName);
            if (userResult.IsError)
            {
                // TODO [StatusMessage]
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                StatusMessage.SetError("Error finding user for unspecified email");
            }
            else if (_emailManager is BureauEmailManager)
            {
                // Once you add a real email sender, you should remove this code that lets you confirm the account

                Result<string> codeResult = await _accountManager.GenerateEmailConfirmationTokenAsync(userResult.Value);
                string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));
                //TODO [PageAddress]
                emailConfirmationLink = NavigationManager.GetUriWithQueryParameters(
                    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                    new Dictionary<string, object?> { ["userId"] = userResult.Value.Id, ["code"] = code, ["returnUrl"] = ReturnUrl });
            }
        }
    }
}
