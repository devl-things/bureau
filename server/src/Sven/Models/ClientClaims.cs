using System.Security.Claims;

namespace Sven.Models
{
    public class ClientClaims : ScopeParameter
    {
        public ClientClaims() { }
        public ClientClaims(ClientClaims clientClaims, string? newScope)
        {
            ClientId = clientClaims.ClientId;
            Claims = clientClaims.Claims;
            Nonce = clientClaims.Nonce;
            Scope = newScope;
        }

        public string ClientId { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = new List<Claim>();

        public string? Nonce { get; set; }



        public string GetClaimValue(string claimType)
        {
            Claim? claim = Claims.FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase));
            return claim?.Value ?? string.Empty;
        }
    }
}
