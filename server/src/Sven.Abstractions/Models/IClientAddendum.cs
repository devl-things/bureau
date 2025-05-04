using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public interface IClientAddendum
    {
        [MaxLength(100)]
        public string? AuthMethod { get; set; }
        [MaxLength(2000)]
        public string? ClientUri { get; set; }
        public List<string>? GrantTypes { get; set; }
        public string? Jwks { get; set; }
        [MaxLength(2000)]
        public string? JwksUri { get; set; }
        [MaxLength(2000)]
        public string? LogoUri { get; set; }
        [MaxLength(2000)]
        public string? PolicyUri { get; set; }
        public List<string>? ResponseTypes { get; set; }
        public string? SoftwareId { get; set; }
        public string? SoftwareVersion { get; set; }
        [MaxLength(2000)]
        public string? TosUri { get; set; }
    }
}
