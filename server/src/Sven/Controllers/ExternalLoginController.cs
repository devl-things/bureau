using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.External.Base)]
    public class ExternalLoginController : ControllerBase
    {
        private static AuthenticationProperties _props = new AuthenticationProperties
        {
            RedirectUri = Endpoints.Connect.AuthorizeContinue
        };

        [HttpGet(Endpoints.External.SignInWithProvider)]
        public IActionResult SignInExternal(string provider)
        {
            if (!TryGetSupportedProvider(provider, out string supportedProvider))
            {
                return BadRequest("Unsupported external provider.");
            }
            return Challenge(_props, supportedProvider);
        }

        private bool TryGetSupportedProvider(string provider, out string supportedProvider)
        {
            supportedProvider = string.Empty;
            if (string.IsNullOrWhiteSpace(provider)) return false;
            supportedProvider = provider.Trim().ToLower();
            return AuthConstants.ExternalSchemes.Exists(supportedProvider);
        }
    }
}