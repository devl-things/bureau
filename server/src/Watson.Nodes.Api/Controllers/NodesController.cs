using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Nodes.Root)]
    public sealed class NodesController : ControllerBase
    {
        private readonly INodeService _nodeService;

        public NodesController(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        [HttpGet(ApiRoutes.ByIdSegment)]
        public async Task<ActionResult<NodeDto>> GetAsync(
            [FromRoute] Guid nodeId,
            CancellationToken cancellationToken = default)
        {
            Node? node = await _nodeService.GetAsync(nodeId, cancellationToken);
            if (node is null)
            {
                return NotFound();
            }

            return Ok(node.ToDto());
        }

        [HttpPatch(ApiRoutes.Nodes.AttributesSegment)]
        public async Task<ActionResult<NodeDto>> PatchAttributesAsync(
            [FromRoute] Guid nodeId,
            [FromBody] PatchNodeAttributesRequest request,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<NodeAttribute> set = (request.Set ?? Array.Empty<AttributeDto>()).Select(a => a.ToDomain());

            IEnumerable<(string Key, string? Locale)> remove = (request.Remove ?? Array.Empty<AttributeKeyDto>())
                .Select(r => (r.Key, r.Locale));

            Node updated = await _nodeService.PatchAttributesAsync(nodeId, set, remove, cancellationToken);
            return Ok(updated.ToDto());
        }
    }
}
