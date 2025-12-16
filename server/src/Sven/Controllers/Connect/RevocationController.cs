using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;

namespace Sven.Controllers.Connect
{
    [ApiController]
    [Route(Endpoints.Connect.Revocation)]
    public class RevocationController : ConnectController
    {
        private readonly IClientProvider _clientProvider;
        private readonly ITokenProvider _tokenProvider;

        public RevocationController(ILogger<RevocationController> logger, IClientProvider clientProvider, ITokenProvider tokenProvider) : base(logger)
        {
            _clientProvider = clientProvider;
            _tokenProvider = tokenProvider;
        }
        [HttpPost]
        [DisableAutoValidation]
        [ServiceFilter(typeof(OAuthValidationFilter))]
        public async Task<IActionResult> RevokeTokenAsync([FromForm] TokenRevocationRequest request, CancellationToken cancellationToken = default)
        {
            Result<Client> clientResult = await _clientProvider.GetClientAsync(request.ClientId!, cancellationToken);
            if (clientResult.IsError)
            {
                _logger.LogResultError(clientResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.InvalidClient, "Client authentication failed");
            }

            Result<bool> removeResult = await _tokenProvider.RevokeAsync(request.Token, request.ClientId!, request.TokenTypeHint, cancellationToken);

            if (removeResult.IsError)
            {
                return removeResult.Error.ToOAuthError();
            }

            return Ok();
        }
    }
}
