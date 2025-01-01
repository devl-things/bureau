using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Contexts;
using Bureau.Data.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Data.Postgres.Configurations
{
    public static class BureauDataPostgresServiceConfiguration
    {
        public static IServiceCollection AddBureauDataPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BureauContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("BureauConnection"));
            });
            services.AddScoped<IRepository, RecordRepository>();

            return services;
        }
    }
}
