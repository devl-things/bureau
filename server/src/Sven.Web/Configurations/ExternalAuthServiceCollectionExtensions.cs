using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven.Services;
using Sven.Services;

namespace Sven.Configurations
{
    public static class ExternalAuthServiceCollectionExtensions
    {
        public static IServiceCollection AddSvenAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<SvenProvidersBuilder> configureProviders)
        {
            var auth = services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = Endpoints.Connect.SignIn;
                    options.ReturnUrlParameter = AuthConstants.PropertyNames.RedirectUrl;
                })
                .AddCookie(AuthConstants.AuthenticationSchemes.External);

            SvenProvidersBuilder providersBuilder = new(services, auth, configuration);
            configureProviders(providersBuilder);

            ExternalProviderRegistry registry = new(
                providersBuilder.ProviderList,
                providersBuilder.ScopesByProvider);
            services.AddSingleton<IExternalProviderRegistry>(registry);

            return services;
        }
    }
}
