using Bureau.Primitives.Features;
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

            // Inject feature scope claims. Empty list = all features (full dev access).
            IEnumerable<string> featureScopes = (options.Features is { Count: > 0 })
                ? options.Features
                : FeatureKeys.AllKeys;

            foreach (string scope in featureScopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            ClaimsIdentity identity = new(claims, "Dev");
            return new ClaimsPrincipal(identity);
        }
    }
}
