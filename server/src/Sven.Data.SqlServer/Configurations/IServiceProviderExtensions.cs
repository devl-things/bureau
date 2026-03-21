using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sven.Data.SqlServer.Contexts;

namespace Sven.Data.SqlServer.Configurations
{
    public static class IServiceProviderExtension
    {
        public static void MigrateSven(this IServiceProvider serviceProvider)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ILogger<SvenContextSqlServer> logger = scope.ServiceProvider.GetRequiredService<ILogger<SvenContextSqlServer>>();
                SvenContextSqlServer db = scope.ServiceProvider.GetRequiredService<SvenContextSqlServer>();

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
