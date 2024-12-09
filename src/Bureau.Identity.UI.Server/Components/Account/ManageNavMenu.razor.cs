using Bureau.Identity.Managers;
using Microsoft.AspNetCore.Components;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ManageNavMenu
    {
        [Inject]
        private ISignInManager _signInManager { get; set; } = default!;

        private bool _hasExternalLogins;

        protected override async Task OnInitializedAsync()
        {
            _hasExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Any();
        }
    }
}
