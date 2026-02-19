using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Projects.Root)]
    public sealed class ProjectsController : NodesControllerBase
    {
        private readonly NodeKind _kind = NodeKind.Project;

        public ProjectsController(ILogger<ProjectsController> logger, INodeService nodeService) : base(logger, nodeService)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateNodeRequest request, CancellationToken cancellationToken = default)
        {
            return await CreateNodeAsync(_kind, request, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAsync([FromQuery] SearchNodesQueryDto request, CancellationToken cancellationToken = default)
        {
            return await SearchAsync(_kind, request, cancellationToken);
        }
    }
}
