using Bureau.AspNetCore.Logging;
using Bureau.AspNetCore.Mappers;
using Bureau.Server.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.Controllers
{
    public class BureauApiControllerBase : ControllerBase
    {
        protected readonly ILogger _logger;

        public BureauApiControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        protected IActionResult ProblemDetailsResponse(int statusCode, ResultError error)
        {
            _logger.LogResultError(error, HttpContext);
            return StatusCode(statusCode, error.ToProblemDetails(HttpContext, statusCode));
        }
        protected IActionResult OkResponse<T>(T value)
        {
            return Ok(new BureauResponse<T>() { Data = value });
        }

    }
}
