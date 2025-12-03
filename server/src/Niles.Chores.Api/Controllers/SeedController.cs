using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Data;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly IChoresSeeder _seeder;
        public SeedController(ILogger<SeedController> logger, IChoresSeeder seeder)
        {
            _logger = logger;
            _seeder = seeder;
        }

        // POST api/seed
        [HttpPost]
        public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _seeder.SeedAsync(cancellationToken);
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
        public async Task<IActionResult> ClearAndSeedAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _seeder.ClearAndSeedAsync(cancellationToken);
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

