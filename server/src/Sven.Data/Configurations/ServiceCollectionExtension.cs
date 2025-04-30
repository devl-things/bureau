using Microsoft.Extensions.DependencyInjection;
using Sven.Abstractions.Services;
using Sven.Data.Stores;

namespace Sven.Data.Configurations
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddSvenData(this IServiceCollection services)
        {
            services.AddTransient<IClientStore, ClientStore>();
            return services;
        }
    }
}
