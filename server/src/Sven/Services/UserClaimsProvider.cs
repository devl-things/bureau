using Bureau.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.JsonWebTokens;
using Sven.Data;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public class UserClaimsProvider : IUserClaimsProvider, ICurrentUserProvider
    {
        private readonly IUserStore _userStore;
        private SvenUser _user = null!;
        private bool _userSet = false;
        public UserClaimsProvider(IUserStore userStore)
        {
            _userStore = userStore;
        }

        public SvenUser CurrentUser { get { return _userSet ? _user : null!; } }

        public bool IsAuthenticated { get { return _userSet && _user != null; } }

        public async Task<Result<ClaimsPrincipal>> GetClaimsPrincipalAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            Result<SvenUser> userResult = await _userStore.GetByUsernameAsync(username, cancellationToken);
            if (userResult.IsError)
            {
                return userResult.Error;
            }
            if (!PasswordHasher.VerifyPassword(userResult.Value.PasswordHash, password))
            {
                return new ResultError("Password not matching");
            }
            return CreatePrincipal(userResult.Value);
        }

        public async Task<Result<ClaimsPrincipal>> GetClaimsPrincipalAsync(string userId, CancellationToken cancellationToken = default)
        {
            Result<SvenUser> userResult = await _userStore.GetByIdentifierAsync(userId, cancellationToken);
            if (userResult.IsError)
            {
                return userResult.Error;
            }
            return CreatePrincipal(userResult.Value);
        }

        public async Task SetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            string? userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (_user != null && CurrentUser.SubjectId.Equals(userId))
            {
                _userSet = true;
                return;
            }
            if (principal.Identity?.IsAuthenticated != true && string.IsNullOrEmpty(userId))
            {
                _userSet = false;
                return;
            }
            Result<SvenUser> userResult = await _userStore.GetByIdentifierAsync(userId, cancellationToken);
            if (userResult.IsError)
            {
                _userSet = false;
                return;
            }
            _userSet = true;
            _user = userResult.Value;
        }

        private ClaimsPrincipal CreatePrincipal(SvenUser user)
        {
            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.SubjectId),
                new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
                new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
        }
    }
}
