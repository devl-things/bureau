using Bureau;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.IdentityModel.Tokens.Jwt;

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
        private readonly IClientService _clientService;
        private readonly JwtOptions _jwtOptions;
        private readonly RsaSecurityKey _rsaKey;

        public EndSessionController(
            ILogger<EndSessionController> logger,
            IClientService clientService,
            IOptions<JwtOptions> jwtOptions,
            RsaSecurityKey rsaKey)
        {
            _logger = logger;
            _clientService = clientService;
            _jwtOptions = jwtOptions.Value;
            _rsaKey = rsaKey;
        }

        [HttpGet]
        public async Task<IActionResult> EndSessionAsync(
            [FromQuery(Name = "id_token_hint")] string? idTokenHint,
            [FromQuery(Name = "post_logout_redirect_uri")] string? postLogoutRedirectUri,
            [FromQuery(Name = "state")] string? state,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("EndSession requested. PostLogoutRedirectUri: {Uri}", postLogoutRedirectUri);

            if (string.IsNullOrWhiteSpace(idTokenHint))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok(new { logged_out = true });
            }

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = _rsaKey,
                ClockSkew = TimeSpan.Zero,
            };

            string clientId;
            try
            {
                handler.ValidateToken(idTokenHint, validationParams, out SecurityToken _);
                JwtSecurityToken jwt = handler.ReadJwtToken(idTokenHint);
                clientId = jwt.Audiences.FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("EndSession rejected: invalid id_token_hint. {Reason}", ex.Message);
                return BadRequest(new
                {
                    error = AuthConstants.OAuth.Errors.InvalidRequest,
                    error_description = "id_token_hint signature or issuer invalid."
                });
            }

            Result<Client> clientResult = await _clientService.GetClientAsync(clientId, cancellationToken);
            if (clientResult.IsError)
            {
                _logger.LogWarning("EndSession rejected: unknown or inactive client. ClientId: {ClientId}", clientId);
                return BadRequest(new
                {
                    error = AuthConstants.OAuth.Errors.InvalidRequest,
                    error_description = "id_token_hint references unknown client."
                });
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Client client = clientResult.Value;
            if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri)
                && client.PostLogoutRedirectUris != null
                && client.PostLogoutRedirectUris.Contains(postLogoutRedirectUri))
            {
                string redirectUri = string.IsNullOrWhiteSpace(state)
                    ? postLogoutRedirectUri
                    : $"{postLogoutRedirectUri}?state={Uri.EscapeDataString(state)}";
                return Redirect(redirectUri);
            }

            return Ok(new { logged_out = true });
        }
    }
}
