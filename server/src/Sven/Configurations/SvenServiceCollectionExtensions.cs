using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sven.Data;
using Sven.Data.Repositories;
using Sven.Data.Stores;

namespace Sven.Configurations
{
    public static class SvenServiceCollectionExtensions
    {
        public static IServiceCollection AddSvenCore(this IServiceCollection services)
        {
            services.AddScoped<UserRepository>();
            services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
            services.AddScoped<IUserStore>(sp => sp.GetRequiredService<UserRepository>());
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IExternalTokenService, ExternalTokenService>();
            services.AddScoped<IHouseholdService, HouseholdService>();
            services.AddTransient<IStartupFilter, EncryptionKeyStartupFilter>();
            return services;
        }
    }
}
