using Sven.Models;

namespace Sven.Data.Models
{
    internal class ClientAddendum : IClientAddendum
    {
        public string? AuthMethod { get; set; }
        public string? ClientUri { get; set; }
        public List<string>? GrantTypes { get; set; }
        public string? Jwks { get; set; }
        public string? JwksUri { get; set; }
        public string? LogoUri { get; set; }
        public string? PolicyUri { get; set; }
        public List<string>? ResponseTypes { get; set; }
        public string? SoftwareId { get; set; }
        public string? SoftwareVersion { get; set; }
        public string? TosUri { get; set; }
    }
}
