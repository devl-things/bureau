namespace Sven.Models
{
    public class ClientRequest
    {
        public required List<string> RedirectUris { get; set; }
        public string TokenEndpointAuthMethod { get; set; } = string.Empty;
        public required List<string> GrantTypes { get; set; }
        public required List<string> ResponseTypes { get; set; }
        public string? ClientName { get; set; }
        public string? ClientUri { get; set; }
        public string? LogoUri { get; set; }
        public string? Scope { get; set; }
        public List<string>? Contacts { get; set; }
        public string? TosUri { get; set; }
        public string? PolicyUri { get; set; }
        public string? JwksUri { get; set; }
        public string? Jwks { get; set; }
        public string? SoftwareId { get; set; }
        public string? SoftwareVersion { get; set; }
    }
}
