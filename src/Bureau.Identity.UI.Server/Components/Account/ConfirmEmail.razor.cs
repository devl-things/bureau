using Bureau.Core;
using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Bureau.Core.Factories;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ConfirmEmail
    {
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        private string? statusMessage;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromQuery]
        private string? UserId { get; set; }

        [SupplyParameterFromQuery]
        private string? Code { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (UserId is null || Code is null)
            {
                //TODO 
                _redirectManager.RedirectTo("");
            }

            IUserId userId = BureauUserIdFactory.CreateIBureauUserId(UserId);
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            Result result = await _accountManager.ConfirmEmailAsync(userId, code);

            if (result.IsSuccess) 
            {
                //TODO [StatusMessage]
                statusMessage = result.IsSuccess ? "Thank you for confirming your email." : "Error confirming your email.";
            }
            else 
            {
                // TODO [StatusMessage]
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                statusMessage = $"Error loading user with ID {UserId}";
            }
        }
    }
}
