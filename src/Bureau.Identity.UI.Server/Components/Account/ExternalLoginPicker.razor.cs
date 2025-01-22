using Bureau.Identity.Managers;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ExternalLoginPicker
    {
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; } = default!;

        private AuthenticationScheme[] externalLogins = [];

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToArray();
        }
    }
}
