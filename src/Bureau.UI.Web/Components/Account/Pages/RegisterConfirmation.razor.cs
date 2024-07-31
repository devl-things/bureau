using Bureau.UI.Web.Components.Account.Managers;
using Bureau.UI.Web.Components.Shared;
using Bureau.UI.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class RegisterConfirmation
    {
        private string? emailConfirmationLink;

        private StatusMessageInput StatusMessage = new StatusMessageInput();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? UserName { get; set; }

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }
        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; init; }
        [Inject]
        public required IEmailSender<ApplicationUser> EmailSender { get; init; }
        [Inject]
        public required NavigationManager NavigationManager { get; init; }
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; init; }

        protected override async Task OnInitializedAsync()
        {
            if (UserName is null)
            {
                RedirectManager.RedirectTo("");
            }

            var user = await UserManager.FindByNameAsync(UserName);
            if (user is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                StatusMessage.SetError("Error finding user for unspecified email");
            }
            else if (EmailSender is IdentityNoOpEmailSender)
            {
                // Once you add a real email sender, you should remove this code that lets you confirm the account
                var userId = await UserManager.GetUserIdAsync(user);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                emailConfirmationLink = NavigationManager.GetUriWithQueryParameters(
                    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });
            }
        }
    }
}
