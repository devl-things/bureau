using Bureau.Google.Abstractions;
using Bureau.Google.Managers;
using Bureau.Google.Middleware;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Google.Configurations
{
    public static class BureauGoogleServiceConfiguration
    {
        private static readonly HashSet<string> registeredHttpClients = new HashSet<string>();
        public static void AddGoogleApiHttpClient(this IServiceCollection services, Action<GoogleOptions> configureOptions)
        {
            if (!registeredHttpClients.Contains(GoogleHttpClientNames.GoogleApiClientName))
            {
                services.AddSingleton(TimeProvider.System);
                services.AddTransient<IGoogleAccessTokenManager, GoogleAccessTokenManager>();
                services.AddTransient<GoogleAccessTokenDelegatingHandler>();

                services.Configure<GoogleOptions>(configureOptions);

                services.AddHttpClient(GoogleHttpClientNames.GoogleApiClientName, client =>
                {
                    client.BaseAddress = new Uri(GoogleUris.GoogleApiAddress);
                    //client.DefaultRequestHeaders.UserAgent.TryParseAdd(options.ActivityHub.UserAgent);
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    //client.DefaultRequestHeaders.Accept.Clear();
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                })
                .AddHttpMessageHandler<GoogleAccessTokenDelegatingHandler>();

                registeredHttpClients.Add(GoogleHttpClientNames.GoogleApiClientName );
            }
        }
    }
}
