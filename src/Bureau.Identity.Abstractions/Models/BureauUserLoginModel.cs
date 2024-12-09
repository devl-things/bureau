namespace Bureau.Identity.Models
{
    public class BureauUserLoginModel : IBureauLoginModel
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string? ProviderDisplayName { get; set; }
        /// <summary>
        /// Creates a new instance of <see cref="UserLoginInfo"/>
        /// </summary>
        /// <param name="loginProvider">The provider associated with this login information.</param>
        /// <param name="providerKey">The unique identifier for this user provided by the login provider.</param>
        /// <param name="displayName">The display name for the login provider.</param>
        public BureauUserLoginModel(string loginProvider, string providerKey, string? displayName)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            ProviderDisplayName = displayName;
        }

        /// <summary>
        /// User Id
        /// </summary>
        public string UserId { get; set; } = default!;
        public string? Email { get; set; }

        public BureauUserLoginModel(string loginProvider, string providerKey, string? displayName, string userId, string? email) : this(loginProvider, providerKey, displayName)
        {
            UserId = userId;
            Email = email;
        }

        //[Obsolete]
        //public BureauUserLoginModel(string loginProvider, string providerKey, string? displayName) : base(loginProvider, providerKey, displayName)
        //{
        //}

    }
}
