using Bureau;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Data;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IChoresSeeder _seeder;
        public SeedController(IChoresSeeder seeder)
        {
            _seeder = seeder;
        }

        // POST api/seed
        [HttpPost]
        public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
        {
            Result seedResult = await _seeder.SeedAsync(cancellationToken);

            if (seedResult.IsError)
            {
                return StatusCode(500, new { error = "Database was not seeded.", details = seedResult.Error.ErrorMessage });
            }
            return Ok(new { message = "Database seeded successfully!" });
        }

        // POST api/seed/clear
        [HttpPost("clear")]
        public async Task<IActionResult> ClearAndSeedAsync(CancellationToken cancellationToken)
        {
            Result seedResult = await _seeder.ClearAndSeedAsync(cancellationToken);
            if (seedResult.IsError)
            {
                return StatusCode(500, new { error = "Database was not cleared and seeded.", details = seedResult.Error.ErrorMessage });
            }
            return Ok(new { message = "Database seeded successfully!" });
        }
    }
}

