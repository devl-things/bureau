using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sven.Data.Postgres.Contexts;

namespace Sven.Data.Postgres.Configurations
{
    public static class IServiceProviderExtension
    {
        public static void MigrateSven(this IServiceProvider serviceProvider)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ILogger<SvenContextPostgres> logger = scope.ServiceProvider.GetRequiredService<ILogger<SvenContextPostgres>>();
                SvenContextPostgres db = scope.ServiceProvider.GetRequiredService<SvenContextPostgres>();

                try
                {
                    logger.LogInformation("Applying Sven database migrations...");
                    db.Database.Migrate();
                    logger.LogInformation("Sven database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Error applying Sven database migrations.");
                }
            }
        }
    }
}
