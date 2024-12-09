using Bureau.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;

namespace Bureau.UI.Components
{
    [Authorize]
    public class AuthComponentBase : BureauComponentBase
    {
        //[CascadingParameter]
        //private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        //protected ClaimsPrincipal UserClaimsPrincipal { get; set; } = default!;
        //protected IUserId UserId { get; set; } = default!;



        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            //UserClaimsPrincipal = (await AuthenticationStateTask!).User;

            //UserId = new BureauUserId(UserClaimsPrincipal.Claims
            //    .Where(x => x.ValueType == ClaimTypes.NameIdentifier)
            //    .Select(x => x.Value)
            //    .FirstOrDefault());

        }
    }
}
