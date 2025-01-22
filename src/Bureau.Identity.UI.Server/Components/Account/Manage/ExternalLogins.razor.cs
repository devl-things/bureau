using Bureau.UI.Managers;
using Microsoft.AspNetCore.Identity;
using Bureau.Identity.UI.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Bureau.Identity.Models;
using Bureau.Core;
using Bureau.Identity.Managers;
using Bureau.Identity.Providers;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class ExternalLogins
    {
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;
        [Inject]
        private IUserManager _userManager { get; set; } = default!;
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;

        private BureauUser user = default!;
        private List<BureauUserLoginModel>? currentLogins;
        private IList<AuthenticationScheme>? otherLogins;
        private bool showRemoveButton;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        //TODO [SupplyParameterFromForm]
        [SupplyParameterFromForm]
        private string? LoginProvider { get; set; }

        [SupplyParameterFromForm]
        private string? ProviderKey { get; set; }

        [SupplyParameterFromQuery]
        private string? Action { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Result<BureauUser> userResult = await _identityProvider.GetUserAsync();

            if (userResult.IsError) 
            {
                //TODO [error handling]
                return;
            }
            user = userResult.Value;
            Result<List<BureauUserLoginModel>> currentLoginsResult = await _userManager.GetUserLoginsAsync(userResult.Value);
            currentLogins = currentLoginsResult.Value;

            otherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();


            //TODO is password set
            string? passwordHash = null;
            //if (UserStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
            //{
            //    passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
            //}

            showRemoveButton = passwordHash is not null || currentLogins.Count > 1;

            if (HttpMethods.IsGet(HttpContext.Request.Method) && Action == BureauIdentityCallbackActionNames.LinkLoginCallbackAction)
            {
                await OnGetLinkLoginCallbackAsync();
            }
        }

        private async Task OnSubmitAsync()
        {

            Result result = await _userManager.RemoveUserLoginAsync(user, new BureauLoginModel(LoginProvider!, ProviderKey!));
            if (result.IsError)
            {
                _redirectManager.RedirectToCurrentPageWithStatus("Error: The external login was not removed.", HttpContext);
            }

            Result refreshSignInResult = await _signInManager.RefreshSignInAsync(user);
            if (refreshSignInResult.IsError)
            {
                // TODO [StatusMessage] Show status in status message
                _redirectManager.RedirectToCurrentPageWithStatus("Error: The external login was not removed.", HttpContext);
                return;
            }
            _redirectManager.RedirectToCurrentPageWithStatus("The external login was removed.", HttpContext);
        }

        private async Task OnGetLinkLoginCallbackAsync()
        {
            Result<BureauExternalLogin> infoResult = await _signInManager.GetExternalLoginAsync(user.Id);
            if (infoResult.IsError)
            {
                //TODO [StatusMessage]
                _redirectManager.RedirectToCurrentPageWithStatus("Error: Could not load external login info.", HttpContext);
            }

            Result result = await _userManager.AddUserLoginAsync(user, infoResult.Value);
            if (result.IsError)
            {
                _redirectManager.RedirectToCurrentPageWithStatus("Error: The external login was not added. External logins can only be associated with one account.", HttpContext);
                return;
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            _redirectManager.RedirectToCurrentPageWithStatus("The external login was added.", HttpContext);
        }
    }
}
