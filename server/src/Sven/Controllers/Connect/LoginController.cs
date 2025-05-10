using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Services;
using System.Net;
using System.Security.Claims;

namespace Sven.Controllers.Connect
{
    [ApiController]
    [Route(Endpoints.Connect.Login)]
    public class LoginController : ConnectController
    {
        private readonly IUserClaimsProvider _userProvider;

        public LoginController(ILogger<LoginController> logger, IUserClaimsProvider userProvider) : base(logger)
        {
            _userProvider = userProvider;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken = default)
        {
            Result<ClaimsPrincipal> claimsPrincipalResult = await _userProvider.GetClaimsPrincipalAsync(username, password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                return OAuthError(AuthConstants.OAuth.Errors.AccessDenied, AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword, HttpStatusCode.Unauthorized);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);

            return Redirect(Endpoints.Account.AccountInfo);
        }
    }
}
