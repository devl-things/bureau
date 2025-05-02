using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Models;

namespace Sven.Controllers
{
    public partial class ConnectController
    {
        [HttpPost(Endpoints.Connect.RevocationPath)]
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
                return OAuthError(removeResult.Error);
            }

            return Ok();
        }
    }
}
