using Bureau.Identity.UI.Client.Mock.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Identity.UI.Client.Configurations
{
    public static class BureauIdentityUIClientServiceConfiguration
    {
        public static void AddMockBureauIdentityUIClient(this IServiceCollection services) 
        {
            services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();
        }
    }
}
