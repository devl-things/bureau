using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.External.Base)]
    public class ExternalLoginController : BureauApiControllerBase
    {
        private readonly AuthenticationProperties _props = new AuthenticationProperties
        {
            RedirectUri = Endpoints.Connect.AuthorizePlain
        };

        public ExternalLoginController(ILogger<ExternalLoginController> logger) : base(logger)
        {
        }

        [HttpGet(Endpoints.External.SignInWithProvider)]
        public IActionResult SignInExternal([FromRoute] string provider,
            [FromQuery(Name = AuthConstants.PropertyNames.Mode)] string? mode,
            [FromQuery(Name = AuthConstants.PropertyNames.RedirectUrl)] string? returnUrl)
        {
            if (!TryGetSupportedProvider(provider, out string supportedProvider))
            {
                //#52
                return BadRequest("Unsupported external provider.");
            }

            string redirectUri = Endpoints.Connect.AuthorizePlain;
            if (!string.IsNullOrWhiteSpace(mode))
            {
                switch (mode)
                {
                    case Modes.ExternalLogin.Pkce:
                        redirectUri = Endpoints.Connect.AuthorizePkce;
                        break;
                    default:
                        break;
                }
            }
            AuthenticationProperties props = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                props.Items[AuthConstants.PropertyNames.RedirectUrl] = returnUrl;
            }

            return Challenge(props, supportedProvider);
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