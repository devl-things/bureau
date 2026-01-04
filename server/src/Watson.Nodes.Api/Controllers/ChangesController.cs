using Bureau;
using Bureau.Server.Contracts;
using Bureau.Server.Contracts.Mappers;
using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Abstractions.Services;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Changes.Root)]
    public sealed class ChangesController : ControllerBase
    {
        private readonly IChangeFeedService _changeFeedService;

        public ChangesController(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] long after = 0,
            [FromQuery] int limit = 500,
            [FromQuery] ChangeModeContract mode = ChangeModeContract.Compact,
            CancellationToken cancellationToken = default)
        {
            int safeLimit = Math.Clamp(limit, 1, 2000);

            CursorResult<ChangeEvent> feed = await _changeFeedService.GetChangesAsync(after, safeLimit, mode.ToDomain(), cancellationToken);

            BureauCursorResponse<ChangeEventDto> dto = feed.ToCursorResponse(x => x.ToDto());

            return Ok(dto);
        }
    }
}
