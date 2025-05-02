using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Models;
using System.Net.Mime;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.Oidc.Base)]
    public class OidcController : ControllerBase
    {
        [HttpPost(Endpoints.Oidc.RegisterPath)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RegisterAsync([FromBody] ClientRegistrationRequest request, CancellationToken cancellationToken = default)
        {
            //var validationResult = await _clientProvider.ValidateRegistrationAsync(request, cancellationToken);
            //if (validationResult.IsError)
            //{
            //    return BadRequest(validationResult.Error);
            //}

            //var registrationResult = await _clientProvider.RegisterClientAsync(request, cancellationToken);
            //if (registrationResult.IsError)
            //{
            //    return StatusCode(500, registrationResult.Error);
            //}
            return Created(uri: $"{Endpoints.Oidc.Register}",
                new ClientRegistrationResponse()
                {
                    ClientId = Guid.NewGuid().ToString(),
                });
        }
    }
}
