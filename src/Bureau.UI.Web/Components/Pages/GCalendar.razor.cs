using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;


namespace Bureau.UI.Web.Components.Pages
{
    public partial class GCalendar
    {
        private string _message;
        //[Inject]
        //private IOptionsMonitor<GoogleOptions> _gOptions { get; set; }

        //[Inject]
        //private IGoogleAuthProvider GoogleAuth { get; set; }
        [Inject]
        private IUserManager _userManager { get; init; }
        //[Inject]
        //private SignInManager<ApplicationUser> _signInManager { get; init; }


        [CascadingParameter]
        private Task<AuthenticationState> AuthStateTask { get; set; }

        protected override Task OnInitializedAsync()
        {
            _message = "What";
            return base.OnInitializedAsync();
        }
        private async Task DoMagic()
        {
            try
            {
                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("https://www.googleapis.com/calendar");

                AuthenticationState authState = await AuthStateTask;

                _message = authState.User.ToString();
                //var user = _userManager.Users.SingleOrDefault(x => x.Id == "myId");
                //if (user == null)
                //    return;

                //var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
                //ApplicationUser? user = await _userManager.GetUserAsync(authState.User);

                //IList<BureauUserLoginModel> logins = await _userManager.GetUserLoginsAsync(user!);

                //BureauUserLoginModel one = logins.First();
                //string? externalAccessToken = await _userManager.GetAuthenticationTokenAsync(user!, one.LoginProvider,one.ProviderKey, OpenIdConnectParameterNames.AccessToken);
                //string? externalAccessTokenType = await _userManager.GetAuthenticationTokenAsync(user!, one.LoginProvider, one.ProviderKey, OpenIdConnectParameterNames.TokenType);


                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(externalAccessTokenType!, externalAccessToken);

                //HttpResponseMessage response = await client.GetAsync("https://www.googleapis.com/calendar/v3/users/me/calendarList");
                //_message = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _message = ex.ToString();
            }
        }
        
    }
}
