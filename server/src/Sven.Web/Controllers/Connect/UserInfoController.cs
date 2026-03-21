using Bureau;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven;
using Sven.Services;
using System.Security.Claims;

namespace Sven.Controllers.Connect
{
    /// <summary>
    /// UserInfo endpoint — OIDC Core §5.3
    /// Returns claims about the authenticated user based on the access token scopes.
    /// </summary>
    [ApiController]
    [Route(Endpoints.Oidc.UserInfo)]
    public class UserInfoController : ControllerBase
    {
        private readonly ILogger<UserInfoController> _logger;
        private readonly IUserService _userService;
        private readonly RsaSecurityKey _rsaKey;
        private readonly JwtOptions _jwtOptions;

        public UserInfoController(
            ILogger<UserInfoController> logger,
            IUserService userService,
            RsaSecurityKey rsaKey,
            Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions)
        {
            _logger = logger;
            _userService = userService;
            _rsaKey = rsaKey;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> UserInfoAsync(CancellationToken cancellationToken = default)
        {
            string? bearerToken = ExtractBearerToken();
            if (bearerToken == null)
                return Unauthorized();

            ClaimsPrincipal? principal = await ValidateAccessTokenAsync(bearerToken);
            if (principal == null)
                return Unauthorized();

            string? userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            Result<SvenUser> userResult = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (userResult.IsError)
            {
                _logger.LogWarning("UserInfo: user {UserId} not found.", userId);
                return Unauthorized();
            }

            string scopeClaim = principal.FindFirstValue("scope") ?? string.Empty;
            ScopeParameter scopes = new ScopeParameter(scopeClaim);

            Dictionary<string, object> claims = new()
            {
                [JwtRegisteredClaimNames.Sub] = userId
            };

            if (scopes.HasScope(AuthConstants.Scopes.Profile))
            {
                claims[JwtRegisteredClaimNames.Name] = userResult.Value.DisplayName;
                claims[JwtRegisteredClaimNames.PreferredUsername] = userResult.Value.Username;
            }

            if (scopes.HasScope(AuthConstants.Scopes.Email))
            {
                claims[JwtRegisteredClaimNames.Email] = userResult.Value.Username;
                claims["email_verified"] = true;
            }

            return Ok(claims);
        }

        private string? ExtractBearerToken()
        {
            string? authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;
            return authHeader["Bearer ".Length..].Trim();
        }

        private async Task<ClaimsPrincipal?> ValidateAccessTokenAsync(string token)
        {
            try
            {
                TokenValidationParameters validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = _rsaKey,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                JsonWebTokenHandler handler = new JsonWebTokenHandler();
                TokenValidationResult result = await handler.ValidateTokenAsync(token, validationParams);
                return result.IsValid ? new ClaimsPrincipal(result.ClaimsIdentity) : null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Access token validation failed in UserInfo endpoint.");
                return null;
            }
        }
    }
}
