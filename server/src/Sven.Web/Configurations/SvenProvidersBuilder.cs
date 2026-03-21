using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven;

namespace Sven.Configurations
{
    public sealed class SvenProvidersBuilder
    {
        internal IServiceCollection Services { get; }
        internal AuthenticationBuilder AuthBuilder { get; }
        internal IConfiguration Configuration { get; }
        internal List<RegisteredExternalProvider> ProviderList { get; } = new();
        internal Dictionary<string, List<ExternalScope>> ScopesByProvider { get; } = new();

        internal SvenProvidersBuilder(
            IServiceCollection services,
            AuthenticationBuilder authBuilder,
            IConfiguration configuration)
        {
            Services = services;
            AuthBuilder = authBuilder;
            Configuration = configuration;
        }

        internal void RegisterProvider(RegisteredExternalProvider descriptor, IEnumerable<ExternalScope> scopes)
        {
            ProviderList.Add(descriptor);
            ScopesByProvider[descriptor.ProviderKey] = scopes.ToList();
        }
    }
}
