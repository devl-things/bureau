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
        /// <summary>
        /// as json
        /// </summary>
        public List<string>? PostLogoutRedirectUris { get; set; }
        public bool Active { get; set; }
        public List<string>? Contacts { get; set; }

        public SerializedData ClientAddendum { get; set; } = null!;
        public string? HashedSecret { get; set; }

        public TimeSpan? IdTokenLifetime { get; set; }
        public TimeSpan? RefreshTokenLifetime { get; set; }
        public TimeSpan? AccessTokenLifetime { get; set; }


        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!;

        public ICollection<ClientFeatureDb> ClientFeatures { get; set; } = null!;
    }
}
