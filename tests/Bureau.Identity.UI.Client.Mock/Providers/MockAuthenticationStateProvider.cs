using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Bureau.Identity.UI.Client.Mock.Providers
{
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            List<Claim> claims = new List<Claim>
            {
                new(ClaimTypes.Name, "lokiMock"),
                new(ClaimTypes.Email, "loki@email.com"),
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, nameof(MockAuthenticationStateProvider));
            ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

            //AuthenticationState authState = new AuthenticationState(new ClaimsPrincipal());
            AuthenticationState authState = new AuthenticationState(principal);
            return Task.FromResult(authState);
            //await Task.Delay(5000);
            //return authState;
        }
    }
}
