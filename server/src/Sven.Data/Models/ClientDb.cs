namespace Sven.Data.Models
{
    public class ClientDb
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// as json
        /// </summary>
        public HashSet<string> Scope { get; set; }
        /// <summary>
        /// as json
        /// </summary>
        public HashSet<string> RedirectUris { get; set; }
        public bool Active { get; set; }

        public TimeSpan? IdTokenLifetime { get; set; }
        public TimeSpan? RefreshTokenLifetime { get; set; }
        public TimeSpan? AccessTokenLifetime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!;

    }
}
