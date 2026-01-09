using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Server.Contracts.Mappers;
using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Api.Factories;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Changes.Root)]
    public sealed class ChangesController : BureauApiControllerBase
    {
        private readonly IChangeFeedService _changeFeedService;

        public ChangesController(ILogger<ChangesController> logger, IChangeFeedService changeFeedService) : base(logger)
        {
            _changeFeedService = changeFeedService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] ChangesQueryDto query, CancellationToken cancellationToken = default)
        {
            ChangeFeedQuery request = ChangeFeedQueryFactory.Create(query);

            CursorResult<ChangeEvent> feedResult = await _changeFeedService.GetChangesAsync(request, cancellationToken);

            if (feedResult.IsError)
            {
                return ProblemDetailsResponse(feedResult.Error);
            }
            return Ok(feedResult.ToCursorResponse(x => x.ToDto()));
        }
    }
}
