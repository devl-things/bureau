using Bureau;
using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Nodes.Root)]
    public sealed class NodesController : BureauApiControllerBase
    {
        private readonly INodeService _nodeService;

        public NodesController(ILogger<NodesController> logger, INodeService nodeService) : base(logger)
        {
            _nodeService = nodeService;
        }

        [HttpGet(ApiRoutes.ByIdSegment)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid nodeId, CancellationToken cancellationToken = default)
        {
            Result<Node> result = await _nodeService.GetAsync(nodeId, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return OkResponse(result.Value.ToDto());
        }

        [HttpPatch(ApiRoutes.Nodes.AttributesSegment)]
        public async Task<IActionResult> PatchAttributesAsync([FromRoute] Guid nodeId, [FromBody] PatchNodeAttributesRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            PatchNodeAttributesCommand attributeChanges = new PatchNodeAttributesCommand()
            {
                Set = (request.Set ?? Array.Empty<AttributeDto>()).Select(x => x.ToDomain()),
                Remove = (request.Remove ?? Array.Empty<AttributeKeyDto>()).Select(x => x.ToDomain())
            };
            Result<Node> result = await _nodeService.PatchNodeAttributesAsync(nodeId, attributeChanges, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return OkResponse(result.Value.ToDto());
        }
    }
}
