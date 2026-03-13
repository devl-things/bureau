using Bureau.Primitives.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Watson.Nodes.Configurations;
using Watson.Nodes.Contexts;
using Watson.Nodes.Data.SqlServer.Contexts;
using Watson.Nodes.Data.SqlServer.Health;

namespace Watson.Nodes.Data.SqlServer.Configurations
{
    public static class IServiceCollectionExtension
    {
        public const string WatsonNodesConnectionStringName = "WatsonNodesDb";
        public static IServiceCollection AddWatsonNodesSqlServer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("WatsonNodesDb");

            return services.AddWatsonNodesSqlServer(connectionString);
        }
        public static IServiceCollection AddWatsonNodesSqlServer(this IServiceCollection services, string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string not defined correctly.");
            }
            services.AddDbContext<SqlServerNodesContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(SqlServerNodesContext).Assembly.FullName);
                });
            });

            // Expose as base type so Watson.Nodes services can depend on NodesContext
            services.AddScoped<NodesContext>(sp =>
            {
                SqlServerNodesContext db = sp.GetRequiredService<SqlServerNodesContext>();
                return db;
            });

            services.AddScoped<IHealthProbe, NodesHealthProbe>();

            services.AddWatsonNodesCore();

            return services;
        }
    }
}
