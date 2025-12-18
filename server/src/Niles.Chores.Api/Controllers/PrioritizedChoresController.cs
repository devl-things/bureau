using Bureau.AspNetCore.Controllers;
using Bureau.Server.Contracts;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/housekeeping/prioritized-chores")]
    [ApiController]
    public class PrioritizedChoresController : BureauApiControllerBase
    {
        private readonly IPrioritizedChoreService _prioritizedChoreService;
        private readonly TimeProvider _timeProvider;
        private readonly IIdObfuscator _idObfuscator;

        public PrioritizedChoresController(ILogger<PrioritizedChoresController> logger,
            IPrioritizedChoreService prioritizedChoreService, TimeProvider timeProvider, IIdObfuscator idObfuscator) : base(logger)
        {
            _prioritizedChoreService = prioritizedChoreService;
            _timeProvider = timeProvider;
            _idObfuscator = idObfuscator;
        }

        // GET /api/housekeeping/prioritized-chores?date=2025-11-27
        [HttpGet]
        public async Task<IActionResult> GetChoresAsync([FromQuery] DateOnly? date, CancellationToken cancellationToken = default)
        {
            DateOnly dateOnly = date ?? DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);

            List<PrioritizedChore> pChores = await _prioritizedChoreService.GetPrioritizedChoresAsync(dateOnly, cancellationToken);
            return OkResponse(pChores.Select(c => c.ToDto(_idObfuscator)));
        }
    }
}
