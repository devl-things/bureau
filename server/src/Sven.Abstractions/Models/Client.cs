namespace Sven.Models
{
    public class Client
    {
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc6749#section-2.2
        /// Client identifier
        /// </summary>
        public string Identifier { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// https://datatracker.ietf.org/doc/html/rfc6749#section-2.1
        /// confidential/public
        /// </summary>
        public string Type { get; set; }

        public ScopeParameter Scope { get; set; }

        public HashSet<string> RedirectUris { get; set; }
        public bool Active { get; set; }

        public TimeSpan? IdTokenLifetime { get; set; }
        public TimeSpan? RefreshTokenLifetime { get; set; }
        public TimeSpan? AccessTokenLifetime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
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
