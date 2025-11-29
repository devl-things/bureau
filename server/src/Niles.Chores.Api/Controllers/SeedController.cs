using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SeedController> _logger;

        public SeedController(IServiceProvider serviceProvider, ILogger<SeedController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // POST api/seed
        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                
                // Get ChoresContext using reflection since it's internal
                Type? choresContextType = typeof(Niles.Chores.Configurations.IServiceCollectionExtension)
                    .Assembly
                    .GetType("Niles.Chores.Contexts.ChoresContext");
                
                if (choresContextType == null)
                {
                    return StatusCode(500, new { error = "Could not find ChoresContext type." });
                }

                DbContext? context = scope.ServiceProvider.GetRequiredService(choresContextType) as DbContext;
                if (context == null)
                {
                    return StatusCode(500, new { error = "Could not resolve ChoresContext from service provider." });
                }

                await Niles.Chores.Data.ChoresSeeder.SeedAsync(context);
                
                return Ok(new { message = "Database seeded successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding database");
                return StatusCode(500, new { error = "An error occurred while seeding the database.", details = ex.Message });
            }
        }

        // POST api/seed/clear
        [HttpPost("clear")]
        public async Task<IActionResult> ClearAndSeed()
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                
                // Get ChoresContext using reflection since it's internal
                Type? choresContextType = typeof(Niles.Chores.Configurations.IServiceCollectionExtension)
                    .Assembly
                    .GetType("Niles.Chores.Contexts.ChoresContext");
                
                if (choresContextType == null)
                {
                    return StatusCode(500, new { error = "Could not find ChoresContext type." });
                }

                DbContext? context = scope.ServiceProvider.GetRequiredService(choresContextType) as DbContext;
                if (context == null)
                {
                    return StatusCode(500, new { error = "Could not resolve ChoresContext from service provider." });
                }

                await Niles.Chores.Data.ChoresSeeder.ClearAndSeedAsync(context);
                
                return Ok(new { message = "Database cleared and seeded successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing and seeding database");
                return StatusCode(500, new { error = "An error occurred while clearing and seeding the database.", details = ex.Message });
            }
        }
    }
}

