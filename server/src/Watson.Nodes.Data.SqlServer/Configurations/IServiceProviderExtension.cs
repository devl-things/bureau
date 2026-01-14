using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Watson.Nodes.Data.SqlServer.Contexts;

namespace Watson.Nodes.Data.SqlServer.Configurations
{
    public static class IServiceProviderExtension
    {
        public static void MigrateWatsonNodes(this IServiceProvider serviceProvider)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ILogger<SqlServerNodesContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<SqlServerNodesContext>>();
                SqlServerNodesContext db = scope.ServiceProvider.GetRequiredService<SqlServerNodesContext>();

                try
                {
                    logger.LogInformation("Applying Watson.Nodes database migrations...");
                    db.Database.Migrate();
                    logger.LogInformation("Watson.Nodes database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Error applying Watson.Nodes database migrations.");
                }
            }
        }
    }
}
