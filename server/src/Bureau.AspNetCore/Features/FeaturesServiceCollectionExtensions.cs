using Bureau.Primitives.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.AspNetCore.Features;

public static class FeaturesServiceCollectionExtensions
{
    /// <summary>
    /// Binds FeaturesOptions from the "Features" section in appsettings.
    /// Call this in Program.cs to enable the master feature switch.
    /// </summary>
    public static IServiceCollection AddBureauFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FeaturesOptions>(configuration.GetSection(FeaturesOptions.SectionName));
        return services;
    }
}
