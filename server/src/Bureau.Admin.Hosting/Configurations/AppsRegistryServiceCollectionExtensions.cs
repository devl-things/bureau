using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Admin.Hosting.Configurations
{
    public static class AppsRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddBureauAdminAppsRegistry(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<AppsRegistryOptions>()
                .Bind(configuration.GetSection(AppsRegistryOptions.SectionName))
                .Validate(options => options != null, "Admin apps registry options are required.")
                .Validate(options => options.Apps.All(app => app.Url != null), "Each admin app must define a Url.")
                .Validate(options => options.Apps.All(app => !string.IsNullOrWhiteSpace(app.Key)), "Each admin app must define a Key.")
                .Validate(options => options.Apps.All(app => !string.IsNullOrWhiteSpace(app.Title)), "Each admin app must define a Title.");

            services.AddSingleton<IAppsRegistry, AppsRegistry>();

            return services;
        }
    }
}
