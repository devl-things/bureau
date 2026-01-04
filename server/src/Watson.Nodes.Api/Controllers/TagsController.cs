using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Tags.Root)]
    public sealed class TagsController : ControllerBase
    {
        private readonly INodeService _nodeService;

        public TagsController(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        [HttpPost]
        public async Task<ActionResult<NodeDto>> CreateAsync(
            [FromBody] CreateNodeRequest request,
            CancellationToken cancellationToken = default)
        {
            string scope = string.IsNullOrWhiteSpace(request.Scope) ? "global" : request.Scope!;
            string canonicalKey = request.CanonicalKey ?? Guid.NewGuid().ToString("N");
            IEnumerable<NodeAttribute> attributes = (request.Attributes ?? Array.Empty<AttributeDto>()).Select(a => a.ToDomain());

            Node node = await _nodeService.CreateAsync(NodeKind.Tag, scope, canonicalKey, attributes, cancellationToken);
            NodeDto dto = node.ToDto();

            return Created($"/nodes/{dto.NodeId}", dto);
        }

        [HttpGet]
        public async Task<ActionResult<SearchNodesResponse>> SearchAsync(
            [FromQuery] string? query,
            [FromQuery] string? scope,
            [FromQuery] string? locale,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            int safeTake = Math.Clamp(take, 1, 100);
            IReadOnlyList<Node> nodes = await _nodeService.SearchAsync(NodeKind.Tag, query, scope, locale, safeTake, cancellationToken);

            IReadOnlyList<NodeSummaryDto> items = nodes.Select(n => CreateSummary(n, locale)).ToList();

            SearchNodesResponse response = new SearchNodesResponse
            {
                Items = items,
                Next = null
            };

            return Ok(response);
        }

        private static NodeSummaryDto CreateSummary(Node node, string? preferredLocale)
        {
            (string? label, string? labelLocale) = ItemsController_PickBestLabel(node, preferredLocale);

            return new NodeSummaryDto
            {
                NodeId = node.NodeId,
                Kind = node.Kind.ToContract(),
                Scope = node.Scope,
                CanonicalKey = node.CanonicalKey,
                Status = node.Status.ToContract(),
                Label = label,
                LabelLocale = labelLocale
            };
        }

        // Small duplication to avoid internal controller coupling
        private static (string? Label, string? Locale) ItemsController_PickBestLabel(Node node, string? preferredLocale)
        {
            List<NodeAttribute> labelAttributes = node.Attributes
                .Where(a => string.Equals(a.Key, "label", StringComparison.OrdinalIgnoreCase) && a.Type == AttributeValueType.String)
                .ToList();

            if (!string.IsNullOrWhiteSpace(preferredLocale))
            {
                NodeAttribute? exact = labelAttributes.FirstOrDefault(a => string.Equals(a.Locale, preferredLocale, StringComparison.OrdinalIgnoreCase));
                if (exact is not null)
                {
                    return (exact.ValueString, exact.Locale);
                }
            }

            NodeAttribute? any = labelAttributes.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.ValueString));
            return any is null ? (null, null) : (any.ValueString, any.Locale);
        }
    }
}
