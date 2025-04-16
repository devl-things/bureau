using Bureau.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public class InMemoryUserClaimsProvider : IUserClaimsProvider
    {
        private readonly List<User> _users = new()
        {
            new User { Username = "admin", Password = "admin123", DisplayName = "Administrator" },
            new User { Username = "alice", Password = "alice123", DisplayName = "Alice" }
        };

        public Task<Result<ClaimsPrincipal>> GetClaimsPrincipalAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            User? user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && u.Password == password);
            if (user == null) return Task.FromResult(new Result<ClaimsPrincipal>(new ResultError("Username and password not valid")));

            return Task.FromResult(new Result<ClaimsPrincipal>(CreatePrincipal(user)));
        }

        private ClaimsPrincipal CreatePrincipal(User user)
        {
            List<Claim> claims = new()
            {
                new Claim("sub", user.SubjectId),
                new Claim("name", user.DisplayName),
                new Claim("username", user.Username)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
        }
    }
}
