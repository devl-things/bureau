using System.Data;
using System.Security.Claims;

namespace Bureau.Server.Hosting.Dev
{
    public sealed class DevPrincipalFactory : IDevPrincipalFactory
    {
        public ClaimsPrincipal CreatePrincipal(DevAuthOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, options.UserName ?? "dev"),
                new Claim(ClaimTypes.Email, options.Email ?? "dev@local")
            };

            if (options.Roles != null)
            {
                foreach (string role in options.Roles.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            ClaimsIdentity identity = new(claims, "Dev");
            return new ClaimsPrincipal(identity);
        }
    }
}
