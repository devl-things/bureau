using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Server.Contracts;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Contracts;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route(ApiRoutes.Chores.Root)]
    [ApiController]
    public class CriticalChoresController : BureauApiControllerBase
    {
        private readonly ICriticalChoreService _criticalChoreService;
        private readonly IIdObfuscator _idObfuscator;

        public CriticalChoresController(ILogger<CriticalChoresController> logger,
            ICriticalChoreService criticalChoreService, IIdObfuscator idObfuscator) : base(logger)
        {
            _criticalChoreService = criticalChoreService;
            _idObfuscator = idObfuscator;
        }

        [HttpPost(ApiRoutes.Chores.CriticalSegment)]
        public async Task<IActionResult> MarkCriticalAsync(string id, [FromBody] CriticalChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            Result<int> idResult = _idObfuscator.Decode(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result result = await _criticalChoreService.UpsertCriticalChoreAsync(idResult.Value, dto.Description, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }

        [HttpDelete(ApiRoutes.Chores.CriticalSegment)]
        public async Task<IActionResult> RemoveCriticalAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = _idObfuscator.Decode(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }
            Result result = await _criticalChoreService.DeleteCriticalChoreAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }
    }
}
