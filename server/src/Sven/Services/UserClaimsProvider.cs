using Bureau.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.JsonWebTokens;
using Sven.Data;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public class UserClaimsProvider : IUserClaimsProvider
    {
        private readonly IUserStore _userStore;
        public UserClaimsProvider(IUserStore userStore)
        {
            _userStore = userStore;
        }
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
