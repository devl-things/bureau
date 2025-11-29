using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Niles.Chores.Contexts;

namespace Niles.Chores.Configurations
{
    public static class IServiceProviderExtension
    {
        public static void MigrateChores(this IServiceProvider serviceProvider)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ILogger<ChoresContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<ChoresContext>>();
                ChoresContext db = scope.ServiceProvider.GetRequiredService<ChoresContext>();
                try
                {
                    logger.LogInformation("Applying database migrations...");
                    db.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error applying database migrations.");
                    throw;
                }
            }
        }
    }
}
