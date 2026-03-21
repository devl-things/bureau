using Bureau;
using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Net;

namespace Sven.Controllers.Connect
{
    [ApiController]
    [Route(Endpoints.Oidc.Base)]
    public class IntrospectionController : ConnectController
    {
        private readonly ITokenProvider _tokenProvider;

        public IntrospectionController(ILogger<IntrospectionController> logger, ITokenProvider tokenProvider) : base(logger)
        {
            _tokenProvider = tokenProvider;
        }

        [HttpPost(Endpoints.Oidc.IntrospectPath)]
        [DisableAutoValidation]
        [ServiceFilter(typeof(OAuthValidationFilter))]
        public async Task<IActionResult> IntrospectAsync(
            [FromForm] TokenIntrospectionRequest request,
            CancellationToken cancellationToken = default)
        {
            IClientAuthService clientAuthService = HttpContext.RequestServices.GetRequiredService<IClientAuthService>();
            Result<Client> clientResult = await clientAuthService.AuthenticateClientAsync(Request, cancellationToken);
            if (clientResult.IsError)
            {
                Response.Headers["WWW-Authenticate"] = "Basic realm=\"Sven\"";
                return OAuthError(AuthConstants.OAuth.Errors.InvalidClient, "Client authentication failed.", HttpStatusCode.Unauthorized);
            }

            Result<IntrospectionResponse> result = await _tokenProvider.IntrospectAsync(request.Token, cancellationToken);
            if (result.IsError)
            {
                return result.Error.ToOAuthError();
            }

            return Ok(result.Value);
        }
    }
}
