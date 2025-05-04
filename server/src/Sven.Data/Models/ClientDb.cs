namespace Sven.Data.Models
{
    public class ClientDb : IAuditable
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// as json
        /// </summary>
        public List<string> Scope { get; set; } = null!;
        /// <summary>
        /// as json
        /// </summary>
        public List<string> RedirectUris { get; set; } = null!;
        public bool Active { get; set; }

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

        public TimeSpan? IdTokenLifetime { get; set; }
        public TimeSpan? RefreshTokenLifetime { get; set; }
        public TimeSpan? AccessTokenLifetime { get; set; }


        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!;

    }
}
