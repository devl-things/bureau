using Sven.Configurations;

namespace Sven.Models
{
    public class Client : IClientAddendum
    {
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc6749#section-2.2
        /// Client identifier
        /// </summary>
        public required string Identifier { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc6749#section-2.1
        /// confidential/public
        /// </summary>
        public string Type { get; set; }

        public required ScopeParameter Scope { get; set; }

        public required HashSet<string> RedirectUris { get; set; }
        public List<string>? PostLogoutRedirectUris { get; set; }
        public Dictionary<string, List<string>>? BureauFeatures { get; set; }
        public bool Active { get; set; }

        public TimeSpan? IdTokenLifetime { get; set; }
        public TimeSpan? RefreshTokenLifetime { get; set; }
        public TimeSpan? AccessTokenLifetime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string? ClientSecret { get; set; }
        public DateTimeOffset? ClientSecretExpiresAt { get; set; }
        public string? HashedSecret { get; set; }
        public string? ClientUri { get; set; }
        public List<string>? Contacts { get; set; }
        public List<string>? GrantTypes { get; set; }
        public string? Jwks { get; set; }
        public string? JwksUri { get; set; }
        public string? LogoUri { get; set; }
        public string? PolicyUri { get; set; }
        public List<string>? ResponseTypes { get; set; }
        public string? SoftwareId { get; set; }
        public string? SoftwareVersion { get; set; }
        public string? AuthMethod { get; set; }
        public string? TosUri { get; set; }

        public Client()
        {
            Type = AuthConstants.ClientTypes.Public;
            Active = true;
        }

        public bool IsScopeGranted(string scope)
        {
            return Scope.IsScopeSameOrSubset(scope);
        }

        public bool IsValidRedirectUri(string redirectUri)
        {
            return RedirectUris.Contains(redirectUri);
        }

    }
}
