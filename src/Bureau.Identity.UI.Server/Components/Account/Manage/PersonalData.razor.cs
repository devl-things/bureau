using Bureau.Core;
using Bureau.Identity.Providers;
using Microsoft.AspNetCore.Components;

namespace Bureau.Identity.UI.Server.Components.Account.Manage
{
    public partial class PersonalData
    {
        [Inject]
        private IIdentityProvider _identityProvider { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Result<bool> userResult = _identityProvider.IsUserAuthenticated();
            if (userResult.IsError || !userResult.Value)
            { 
                //TODO redirect somewhere
            }
        }
    }
}
