using Microsoft.Extensions.DependencyInjection;

namespace Sven.Data.Configurations
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSvenData(this IServiceCollection services)
        {
            return services;
        }
    }
}
