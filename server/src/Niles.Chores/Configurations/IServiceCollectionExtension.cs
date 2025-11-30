using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Contexts;
using Niles.Chores.Data;
using Niles.Chores.Services;

namespace Niles.Chores.Configurations
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddChores(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string not defined correctly.");
            }
            services.AddDbContext<ChoresContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IChoresHealthService, ChoresHealthService>();
            services.AddScoped<IChoresSeeder, ChoresSeeder>();

            services.AddMemoryCache();

            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<IHouseKeepingService, HouseKeepingService>();
            services.AddScoped<IChoreService, ChoreService>();
            services.AddScoped<IPrioritizedChoreService, PrioritizedChoreService>();
            services.AddScoped<ICriticalChoreService, CriticalChoreService>();
            return services;
        }
    }
}
