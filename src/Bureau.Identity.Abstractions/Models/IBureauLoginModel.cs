namespace Bureau.Identity.Models
{
    public interface IBureauLoginModel
    {
        /// <summary>
        /// Gets or sets the provider for this instance of <see cref="UserLoginInfo"/>.
        /// </summary>
        /// <value>The provider for the this instance of <see cref="UserLoginInfo"/></value>
        /// <remarks>
        /// Examples of the provider may be Local, Facebook, Google, etc.
        /// </remarks>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user identity user provided by the login provider.
        /// </summary>
        /// <value>
        /// The unique identifier for the user identity user provided by the login provider.
        /// </value>
        /// <remarks>
        /// This would be unique per provider, examples may be @microsoft as a Twitter provider key.
        /// </remarks>
        public string ProviderKey { get; set; }
        /// <summary>
        /// Gets or sets the display name for the provider.
        /// </summary>
        /// <value>
        /// The display name for the provider.
        /// </value>
        /// <remarks>
        /// Examples of the display name may be local, FACEBOOK, Google, etc.
        /// </remarks>
        public string? ProviderDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address for this user login.
        /// </summary>
        public string? Email { get; }

    }

    public struct BureauLoginModel : IBureauLoginModel
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string? ProviderDisplayName { get; set; }
        public string? Email { get; set; }
        public BureauLoginModel(string loginProvider, string providerKey)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }
    }
}
