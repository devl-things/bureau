using Bureau.Core.Models.Data;
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
            string? connectionString = configuration.GetConnectionString("BureauConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) 
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            services.AddOptions<BureauDataOptions>()
                .Configure(options => 
                {
                    options.ConnectionString = connectionString;
                });
            services.AddDbContext<BureauContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            services.AddScoped<IRecordCommandRepository, RecordCommandRepository>();
            services.AddScoped<ITermRepository, RecordCommandRepository>();
            services.AddScoped<IRecordQueryRepository<EdgeTypeSearchRequest, BaseAggregateModel>, EdgeTypeSearchQueryRepository>();
            services.AddScoped<IRecordQueryRepository<IdSearchRequest, AggregateModel>, IdSearchQueryRepository>();

            return services;
        }
    }
}
