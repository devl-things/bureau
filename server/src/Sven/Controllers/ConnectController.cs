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
        public static class ErrorMessages
        {
            public const string InvalidRequest = "Invalid request";
        }

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
        public async Task<IActionResult> AuthorizeAsync([FromQuery] AuthorizeRequest request, CancellationToken cancellationToken = default)
        {
            if (!AuthConstants.OAuth.ResponseType.Code.Equals(request.ResponseType, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Response type not supported");
            }

            if (!_authCodeManager.IsCodeChallengeValid(request.CodeChallenge, request.CodeChallengeMethod))
            {
                return BadRequest("Code challenge not properly defined");
            }

            Result<bool> isClientValidResult = await _clientProvider.IsValidAsync(request.ClientId, request.RedirectUri, request.Scope, cancellationToken);

            if (isClientValidResult.IsError || !isClientValidResult.Value)
            {
                return BadRequest("Client invalid.");
            }

            OAuthRequest oauthRequest = new OAuthRequest
            {
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                Scope = request.Scope,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                State = request.State,
                Nonce = request.Nonce,
            };

            string pkceKey = Guid.NewGuid().ToString("N");
            await _pkceRequestStore.StoreAsync(pkceKey, oauthRequest, cancellationToken);

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

            if (!_pkceRequestStore.Exists(pkceKey!))
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
        public async Task<IActionResult> TokenAsync([FromForm] TokenRequest request, CancellationToken cancellationToken = default
        )
        {
            if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantType.AuthorizationCode))
            {
                return await HandleAuthorizationCodeFlow(request, cancellationToken);
            }
            else if (IsGrantType(request.GrantType, AuthConstants.OAuth.GrantType.RefreshToken))
            {
                return await HandleRefreshTokenFlow(request, cancellationToken);
            }

            return BadRequest("Grant type not supported.");
        }

        private async Task<IActionResult> HandleRefreshTokenFlow(TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(ErrorMessages.InvalidRequest);
            }
            Result<bool> isValidResult = await _tokenProvider.IsRefreshTokenValidAsync(request.RefreshToken!, request.ClientId, request.Scope, cancellationToken);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return BadRequest(ErrorMessages.InvalidRequest);
            }

            Result<SvenToken> jwtResult = await _tokenProvider.CreateTokenAsync(request.RefreshToken!, request.Scope, cancellationToken);

            return HandleTokenResult(jwtResult);
        }

        private async Task<IActionResult> HandleAuthorizationCodeFlow(TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.CodeVerifier))
            {
                return BadRequest(ErrorMessages.InvalidRequest);
            }

            Result<AuthCode> authCodeResult = await _authCodeManager.GetAsync(request.Code!, cancellationToken);
            if (authCodeResult.IsError)
            {
                _logger.LogResultError(authCodeResult.Error);
                return BadRequest(ErrorMessages.InvalidRequest);
            }

            Result<bool> isValidResult = _authCodeManager.IsAuthCodeValid(authCodeResult.Value, request.ClientId, request.CodeVerifier!);

            if (isValidResult.IsError || !isValidResult.Value)
            {
                _logger.LogResultError(isValidResult.Error);
                return BadRequest(ErrorMessages.InvalidRequest);
            }

            Result<SvenToken> jwtResult = await _tokenProvider.CreateTokenAsync(authCodeResult.Value, cancellationToken);

            await _authCodeManager.ClearAsync(request.Code!, cancellationToken);

            return HandleTokenResult(jwtResult);
        }

        private IActionResult HandleTokenResult(Result<SvenToken> result)
        {
            if (result.IsError)
            {
                _logger.LogResultError(result.Error);
                return BadRequest(ErrorMessages.InvalidRequest);
            }
            return Ok(result.Value);
        }

        private static bool IsGrantType(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}