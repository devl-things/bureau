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
    public class NodesControllerBase : BureauApiControllerBase
    {
        protected readonly INodeService _nodeService;
        public NodesControllerBase(ILogger logger, INodeService nodeService) : base(logger)
        {
            _nodeService = nodeService;
        }

        protected async Task<IActionResult> CreateNodeAsync(NodeKind kind, CreateNodeRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            Result<Node> result = await _nodeService.CreateAsync(request.ToDomain(kind), cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            NodeDto dto = result.Value.ToDto();
            return CreatedAtRoute(ApiRoutes.Nodes.ByIdRouteName, new { nodeId = dto.NodeId }, dto);
        }

        protected async Task<IActionResult> SearchAsync(NodeKind kind, SearchNodesQueryDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            SearchNodesQuery query = SearchNodesQueryFactory.Create(kind, request);

            CursorResult<Node> nodesResult = await _nodeService.SearchAsync(query, cancellationToken);

            if (nodesResult.IsError)
            {
                return ProblemDetailsResponse(nodesResult.Error);
            }

            return Ok(nodesResult.ToCursorResponse(x => x.ToDto()));
        }
    }
}
