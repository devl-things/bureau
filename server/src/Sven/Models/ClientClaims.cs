using System.Security.Claims;

namespace Sven.Models
{
    public class ClientClaims
    {
        public string ClientId { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}
