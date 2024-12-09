using Microsoft.AspNetCore.Identity;

namespace Bureau.Core.Identity.Models
{
    public class BureauUserLoginInfo : UserLoginInfo
    {
        /// <summary>
        /// Gets or sets the email address for this user login.
        /// </summary>
        public string? Email { get; set; }

        public BureauUserLoginInfo(string loginProvider, string providerKey, string? displayName, string? email) : base(loginProvider, providerKey, displayName)
        {
            Email = email;
        }

        [Obsolete]
        public BureauUserLoginInfo(string loginProvider, string providerKey, string? displayName) : base(loginProvider, providerKey, displayName)
        {
        }

    }
}
