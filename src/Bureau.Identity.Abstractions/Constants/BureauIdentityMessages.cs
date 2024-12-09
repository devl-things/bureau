namespace Bureau.Identity.Constants
{
    public class BureauIdentityMessages
    {
        #region information
        public const string UserLogin = "User {0} logged in.";
        public const string UserLockedOut = "User {0} locked out.";
        public const string UserCreatedWithPassword = "User {0} created a new account with password.";
        public const string UserCreatedWithProvider = "User {0} created an account using {1} provider.";
        public const string UserLoginInWithProvider = "User {Name} logged in with {LoginProvider} provider.";
        public const string UserTriedDuplicatingAccount = "User from provider {0} with key {1}, tried to link the account with {2}, but there is already other user {3} with that address.";
        public const string LoginAlreadyAssociatedWithAccount = "Login is already associated with existing account";

        #endregion

        #region errors

        public const string EmailChange = "Error: Invalid email change confirmation link.";
        public const string BureauIdentityCriticalError = $"This is really not good. {nameof(Bureau.Identity)} is not set right";
        public const string BureauUserLoginUnauthorized = $"Authentication setup unsuccessful. Request is missing {HttpMessageOptionNames.BureauUserLoginAuth} option.";
        public const string EmailStoreNotSupported = "The default UI requires a user store with email support.";
        public const string ErrorFromExternalProvider = "Error from external provider: {0}";
        public const string ErrorLoadingExternalLogin = "Error loading external login information.";
        #endregion
    }
}
