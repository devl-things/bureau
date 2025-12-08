using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/housekeeping/prioritized-chores")]
    [ApiController]
    public class PrioritizedChoresController : ControllerBase
    {
        private readonly IPrioritizedChoreService _prioritizedChoreService;

        public PrioritizedChoresController(IPrioritizedChoreService prioritizedChoreService)
        {
            _prioritizedChoreService = prioritizedChoreService;
        }

        // GET /api/housekeeping/prioritized-chores?date=2025-11-27
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrioritizedChoreDto>>> GetChoresAsync([FromQuery] DateOnly? date, CancellationToken cancellationToken = default)
        {
            DateOnly dateOnly = date ?? DateOnly.FromDateTime(DateTime.Now);

            List<PrioritizedChore> pChores = await _prioritizedChoreService.GetPrioritizedChoresAsync(dateOnly, cancellationToken);

            return Ok(pChores.Select(c => c.ToDto()));
        }
    }
}
