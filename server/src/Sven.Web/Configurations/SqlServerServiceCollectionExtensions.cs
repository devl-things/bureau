using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using Sven.Data.SqlServer.Configurations;
using Sven.Data.SqlServer.Contexts;

namespace Sven.Web.Configurations
{
    public static class SqlServerServiceCollectionExtensions
    {
        public const string SvenDbConnectionStringName = "SvenDb";

        public static IServiceCollection AddSvenSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString(SvenDbConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string 'SvenDb' is not configured.");
            }

            services.AddDbContext<SvenContextSqlServer>(options =>
                options.UseSqlServer(connectionString,
                    o => o.MigrationsAssembly(typeof(SvenContextSqlServer).Assembly.FullName)));

            services.AddScoped<Sven.Data.Contexts.SvenContext>(sp =>
            {
                SvenContextSqlServer db = sp.GetRequiredService<SvenContextSqlServer>();
                return db;
            });

            services.AddSvenCore(configuration);

            return services;
        }
    }
}
