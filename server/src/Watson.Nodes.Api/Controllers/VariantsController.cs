using Microsoft.AspNetCore.Mvc;
using Watson.Nodes.Contracts;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Services;

namespace Watson.Nodes.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Variants.Root)]
    public class VariantsController : NodesControllerBase
    {
        private readonly NodeKind _kind = NodeKind.Variant;
        public VariantsController(ILogger logger, INodeService nodeService) : base(logger, nodeService)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateNodeRequest request, CancellationToken cancellationToken = default)
        {
            return await CreateNodeAsync(_kind, request, cancellationToken);
        }
    }
}
