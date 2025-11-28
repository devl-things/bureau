using Microsoft.AspNetCore.Mvc;
using Niles.Chores;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/housekeeping/prioritized-chores")]
    [ApiController]
    public class PrioritizedChoresController : ControllerBase
    {
        IPrioritizedChoreService _prioritizedChoreService;

        public PrioritizedChoresController(IPrioritizedChoreService prioritizedChoreService)
        {
            _prioritizedChoreService = prioritizedChoreService;
        }

        // GET /api/housekeeping/prioritized-chores?date=2025-11-27
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChoreDto>>> GetChoresAsync([FromQuery] DateOnly? date, CancellationToken cancellationToken = default)
        {
            DateOnly dateOnly = date ?? DateOnly.FromDateTime(DateTime.Now);

            List<PrioritizedChore> pChores = await _prioritizedChoreService.GetPrioritizedChoresAsync(dateOnly, cancellationToken);

            return Ok(pChores.Select(c => new ChoreDto
            {
                Id = IdObfuscator.Encode(c.Id),
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                Priority = c.Priority,
                Type = c.Type.ToString(),
                WeeklyInterval = c.WeeklyInterval
            }));
        }
    }
}
