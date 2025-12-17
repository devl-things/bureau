using Bureau;
using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Data;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : BureauApiControllerBase
    {
        private readonly IChoresSeeder _seeder;
        public SeedController(ILogger<SeedController> logger, IChoresSeeder seeder) : base(logger)
        {
            _seeder = seeder;
        }

        [HttpPost]
        public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
        {
            Result<SeedOutcome> seedResult = await _seeder.SeedChoresAsync(cancellationToken);

            if (seedResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, seedResult.Error);
            }
            return Ok(new { data = seedResult.Value.ToString() });
        }

        [HttpPost("test")]
        public async Task<IActionResult> SeedTestAsync(CancellationToken cancellationToken)
        {
            Result<SeedOutcome> seedResult = await _seeder.SeedTestAsync(cancellationToken);

            if (seedResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, seedResult.Error);
            }
            return Ok(new { data = seedResult.Value.ToString() });
        }

        [HttpPost("test/clear")]
        public async Task<IActionResult> ClearAndSeedAsync(CancellationToken cancellationToken)
        {
            Result seedResult = await _seeder.ClearAndSeedTestAsync(cancellationToken);
            if (seedResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, seedResult.Error);
            }
            return Ok();
        }
    }
}

