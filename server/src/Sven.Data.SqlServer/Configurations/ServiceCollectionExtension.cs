using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven.Data.Configurations;
using Sven.Data.Contexts;

namespace Sven.Data.SqlServer.Configurations
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSvenSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("SqlServer");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            services.AddDbContext<SvenContext, SvenContextSqlServer>(options =>
                options.UseSqlServer(connectionString,
                    o => o.MigrationsAssembly("Sven.Data.SqlServer")));

            services.AddSvenData();

            return services;

        }
    }
}
