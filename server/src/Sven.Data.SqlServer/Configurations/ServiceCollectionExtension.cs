using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sven.Data.Configurations;
using Sven.Data.Contexts;

namespace Sven.Data.SqlServer.Configurations
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSvenSqlServer(this IServiceCollection services, Action<SvenDataOptions> configureOptions)
        {
            SvenDataOptions internalOptions = new SvenDataOptions();
            configureOptions.Invoke(internalOptions);
            if (string.IsNullOrWhiteSpace(internalOptions.ConnectionString))
            {
                throw new ArgumentNullException(nameof(configureOptions), "Options not defined correctly.");
            }
            services.AddDbContext<SvenContext, SvenContextSqlServer>(options =>
                options.UseSqlServer(internalOptions.ConnectionString,
                    o => o.MigrationsAssembly("Sven.Data.SqlServer")));

            services.AddSvenData();

            return services;

        }
    }
}
