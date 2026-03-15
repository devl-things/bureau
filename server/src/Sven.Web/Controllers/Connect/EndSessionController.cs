using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;

namespace Sven.Controllers.Connect
{
    /// <summary>
    /// RP-initiated logout — OpenID Connect Session Management §5
    /// https://openid.net/specs/openid-connect-session-1_0.html#RPLogout
    /// </summary>
    [ApiController]
    [Route(Endpoints.Connect.EndSession)]
    public class EndSessionController : ControllerBase
    {
        private readonly ILogger<EndSessionController> _logger;

        public EndSessionController(ILogger<EndSessionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> EndSessionAsync(
            [FromQuery(Name = "id_token_hint")] string? idTokenHint,
            [FromQuery(Name = "post_logout_redirect_uri")] string? postLogoutRedirectUri,
            [FromQuery(Name = "state")] string? state,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EndSession requested. PostLogoutRedirectUri: {Uri}", postLogoutRedirectUri);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri) && IsValidUri(postLogoutRedirectUri))
            {
                string redirectUri = string.IsNullOrWhiteSpace(state)
                    ? postLogoutRedirectUri
                    : $"{postLogoutRedirectUri}?state={Uri.EscapeDataString(state)}";
                return Redirect(redirectUri);
            }

            return Ok(new { logged_out = true });
        }

        private static bool IsValidUri(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed) &&
                   (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp);
        }
    }
}
