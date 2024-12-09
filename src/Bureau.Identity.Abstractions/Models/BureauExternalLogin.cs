using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Bureau.Identity.Models
{
    public class BureauExternalLogin : IBureauLoginModel
    {
        public BureauExternalLogin()
        {
            //TODO
        }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string? ProviderDisplayName { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        /// <summary>
        /// The <see cref="AuthenticationToken"/>s associated with this login.
        /// </summary>
        public IEnumerable<AuthenticationToken>? AuthenticationTokens { get; set; }

        /// <summary>
        /// The <see cref="Authentication.AuthenticationProperties"/> associated with this login.
        /// </summary>
        public AuthenticationProperties? AuthenticationProperties { get; set; }

        public string? Email 
        { 
            get
            {
                return TryGetEmail(out string email) ? email : null;
            }
        }

        public bool TryGetEmail(out string email)
        {
            email = string.Empty;
            if (Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = Principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
