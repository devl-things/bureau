using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Server.Hosting.Configurations
{
    public static class AppRuntimeOptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddBureauAppRuntime(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.Configure<AppRuntimeOptions>(configuration.GetSection(AppRuntimeOptions.SectionName));

            return services;
        }
    }
}
