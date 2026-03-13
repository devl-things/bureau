using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Server.Contracts.Mappers;
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
        private readonly INodeEdgeService _nodeEdgeService;

        public NodesController(ILogger<NodesController> logger, INodeService nodeService, INodeEdgeService nodeEdgeService) : base(logger)
        {
            _nodeService = nodeService;
            _nodeEdgeService = nodeEdgeService;
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

        [HttpPost(ApiRoutes.Nodes.EdgesSegment)]
        public async Task<IActionResult> CreateEdgeAsync([FromRoute] Guid nodeId, [FromBody] CreateEdgeRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            CreateEdgeCommand command = request.ToDomain();
            Result<NodeEdge> result = await _nodeEdgeService.AddEdgeAsync(nodeId, command, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return OkResponse(result.Value.ToDto());
        }

        [HttpGet(ApiRoutes.Nodes.EdgesSegment)]
        public async Task<IActionResult> GetEdgesAsync([FromRoute] Guid nodeId, [FromQuery] string? purpose, [FromQuery] long cursor = 0, [FromQuery] int limit = 0, CancellationToken cancellationToken = default)
        {
            CursorParameters cursorParameters = new() { Cursor = cursor };
            cursorParameters.SetLimit(limit);
            SearchEdgesQuery query = new SearchEdgesQuery
            {
                Purpose = purpose,
                Cursor = cursorParameters
            };
            CursorResult<NodeEdge> result = await _nodeEdgeService.GetEdgesAsync(nodeId, query, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return Ok(result.ToCursorResponse(x => x.ToDto()));
        }

        [HttpDelete(ApiRoutes.Nodes.EdgesSegment)]
        public async Task<IActionResult> RemoveEdgeAsync([FromRoute] Guid nodeId, [FromBody] RemoveEdgeRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            RemoveEdgeCommand command = request.ToDomain();
            Result result = await _nodeEdgeService.RemoveEdgeAsync(nodeId, command, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }
    }
}
