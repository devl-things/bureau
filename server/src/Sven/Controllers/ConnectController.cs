using Bureau.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Abstractions.Services;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.Connect.Base)]
    public class ConnectController : ControllerBase
    {
        private readonly ILogger<ConnectController> _logger;

        private readonly AuthCodeProvider _authCodeManager;
        private readonly IStore<string, OAuthRequest> _pkceRequestStore;
        private readonly IUserClaimsProvider _userProvider;
        private readonly IClientProvider _clientProvider;
        private readonly ITokenProvider _tokenProvider;

        public ConnectController(ILogger<ConnectController> logger, AuthCodeProvider authCodeManager, IStore<string, OAuthRequest> pkceRequestStore,
        IUserClaimsProvider userProvider, IClientProvider clientProvider, ITokenProvider tokenProvider)
        {
            _logger = logger;
            _authCodeManager = authCodeManager;
            _pkceRequestStore = pkceRequestStore;
            _userProvider = userProvider;
            _clientProvider = clientProvider;
            _tokenProvider = tokenProvider;
        }

        [HttpGet(Endpoints.Connect.AuthorizePath)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "OAuth protocol")]
        public async Task<IActionResult> AuthorizeRequestAsync(
            [FromQuery] string response_type,
            [FromQuery] string client_id,
            [FromQuery] string redirect_uri,
            [FromQuery] string scope,
            [FromQuery] string code_challenge,
            [FromQuery] string code_challenge_method = AuthConstants.OAuth.CodeChallengeMethods.Sha256,
            [FromQuery] string state = "",
            CancellationToken cancellationToken = default)
        {
            if (!AuthConstants.OAuth.ResponseType.Code.Equals(response_type, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Response type not supported");
            }

            if (!_authCodeManager.IsCodeChallengeValid(code_challenge, code_challenge_method))
            {
                return BadRequest("Code challenge not properly defined");
            }

            Result<bool> isClientValidResult = await _clientProvider.IsValidAsync(client_id, redirect_uri, scope, cancellationToken);

            if (isClientValidResult.IsError || !isClientValidResult.Value)
            {
                return BadRequest("Client invalid.");
            }

            OAuthRequest request = new OAuthRequest
            {
                ClientId = client_id,
                RedirectUri = redirect_uri,
                Scope = scope,
                CodeChallenge = code_challenge,
                CodeChallengeMethod = code_challenge_method,
                State = state
            };

            string pkceKey = Guid.NewGuid().ToString("N");
            await _pkceRequestStore.StoreAsync(pkceKey, request, cancellationToken);

            Response.Cookies.Append(AuthConstants.CookieNames.PkceKey, pkceKey, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return Redirect(Endpoints.Connect.AuthorizePage);
        }

        [HttpPost(Endpoints.Connect.AuthorizeLoginPath)]
        public async Task<IActionResult> LoginAsync([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken = default)
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return BadRequest($"Missing authorization state ({AuthConstants.CookieNames.PkceKey}).");
            }

            if (!_pkceRequestStore.Exists(pkceKey))
            {
                return BadRequest("Invalid or expired authorization request.");
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userProvider.GetClaimsPrincipalAsync(username, password, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                return Unauthorized(claimsPrincipalResult.Error.ErrorMessage);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);

            return Redirect(Endpoints.Connect.AuthorizeContinue);
        }


        [HttpGet(Endpoints.Connect.AuthorizeContinuePath)]
        public async Task<IActionResult> CompleteAuthorizeAsync(CancellationToken cancellationToken = default)
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return BadRequest("Missing authorization state.");
            }
            Result<OAuthRequest> requestResult = await _pkceRequestStore.GetAsync(pkceKey!, cancellationToken);
            if (requestResult.IsError)
            {
                return BadRequest("Invalid or expired authorization state.");
            }
            await _pkceRequestStore.RemoveAsync(pkceKey!, cancellationToken);

            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null || result.Principal.Claims == null)
            {
                return Unauthorized("External login failed.");
            }

            string code = await _authCodeManager.CreateAuthCodeAsync(requestResult.Value, result.Principal.Claims.ToList(), cancellationToken);

            string redirectUrl = $"{requestResult.Value.RedirectUri}?{AuthConstants.OAuth.ResponseType.Code}={code}&state={requestResult.Value.State}";

            return Redirect(redirectUrl);
        }

        [HttpPost(Endpoints.Connect.TokenPath)]
        public async Task<IActionResult> TokenAsync(
            [FromForm] string grant_type,
            [FromForm] string code,
            [FromForm] string code_verifier,
            [FromForm] string client_id,
            CancellationToken cancellationToken = default
        )
        {
            if (!AuthConstants.OAuth.GrantType.AuthorizationCode.Equals(grant_type, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Grant type not supported.");
            }
            Result<AuthCode> authCodeResult = await _authCodeManager.GetAsync(code, cancellationToken);
            if (authCodeResult.IsError)
            {
                _logger.LogResultError(authCodeResult.Error);
                return BadRequest("Invalid code.");
            }

            Result<bool> isValidResult = _authCodeManager.IsAuthCodeValid(authCodeResult.Value, client_id, code_verifier);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return BadRequest("Invalid code.");
            }

            SvenToken jwt = _tokenProvider.CreateToken(authCodeResult.Value.Claims);

            await _authCodeManager.ClearAsync(code, cancellationToken);

            return Ok(jwt);

        }
    }
}