using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Items.Root)]
    public sealed class ItemsController : ControllerBase
    {
        private readonly INodeService _nodeService;
        // TODO repair response
        // TODO check service interface 
        public ItemsController(INodeService nodeService)
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

            Node node = await _nodeService.CreateAsync(NodeKind.Item, scope, canonicalKey, attributes, cancellationToken);
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
            IReadOnlyList<Node> nodes = await _nodeService.SearchAsync(NodeKind.Item, query, scope, locale, safeTake, cancellationToken);

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
            (string? label, string? labelLocale) = PickBestLabel(node, preferredLocale);

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

        private static (string? Label, string? Locale) PickBestLabel(Node node, string? preferredLocale)
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
