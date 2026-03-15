using Sven.Models;

namespace Sven.Models
{
    public class RefreshToken : ClientClaims
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
