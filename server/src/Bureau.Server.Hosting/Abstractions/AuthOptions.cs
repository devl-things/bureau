namespace Bureau.Server.Hosting
{
    public sealed class AuthOptions
    {
        public const string SectionName = "Auth";

        public AuthMode Mode { get; set; } = AuthMode.None;

        public DevAuthOptions Dev { get; set; } = new DevAuthOptions();

        public OidcAuthOptions Oidc { get; set; } = new OidcAuthOptions();
    }

    public sealed class DevAuthOptions
    {
        /// <summary>
        /// UI-only: auto sign-in applies only under this path (e.g. /admin).
        /// Leave empty to apply to all paths.
        /// </summary>
        public string ScopePath { get; set; } = string.Empty;

        public string UserName { get; set; } = "dev";

        public string Email { get; set; } = "dev@local";

        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// API-only: expected bearer token value in Dev mode.
        /// Example header: Authorization: Bearer dev-token
        /// </summary>
        public string ApiToken { get; set; } = "dev-token";
    }

    public sealed class OidcAuthOptions
    {
        /// <summary>
        /// Authority / issuer of Sven (OIDC discovery endpoint is derived from it).
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        // UI (OIDC)
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = "/signin-oidc";
        public List<string> Scopes { get; set; } = new List<string>();

        // API (JWT bearer)
        public string Audience { get; set; } = string.Empty;
    }
}
