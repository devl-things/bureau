using Bureau.Primitives.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using Sven.Data.Contexts;
using Sven.Data.Postgres.Contexts;
using Sven.Data.Postgres.Health;

namespace Sven.Data.Postgres.Configurations
{
    public static class ServiceCollectionExtension
    {
        public const string SvenDbConnectionStringName = "SvenDb";

        public static IServiceCollection AddSvenPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString(SvenDbConnectionStringName);
            return services.AddSvenPostgres(connectionString);
        }

        public static IServiceCollection AddSvenPostgres(this IServiceCollection services, string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string not defined correctly.");
            }

            services.AddDbContext<SvenContextPostgres>(options =>
                options.UseNpgsql(connectionString,
                    o => o.MigrationsAssembly(typeof(SvenContextPostgres).Assembly.FullName)));

            services.AddScoped<SvenContext>(sp =>
            {
                SvenContextPostgres db = sp.GetRequiredService<SvenContextPostgres>();
                return db;
            });

            services.AddScoped<IHealthProbe, SvenHealthProbe>();

            services.AddSvenCore();

            return services;
        }
    }
}
