using Bureau.UI.Web.Data;

namespace Bureau.UI.Web.Utils
{
    internal static class UIMessages
    {
        public const string CriticalError = "This is really not good. Application is not set right";

        public const string AccountDoesntExist = "Such account doesn't exist. Either omit password and create new one, or insert correct username/password combination";

        public const string ExistingUserByWrongPassword = @"There is an existing account, however, that account has no password set. 
            Please log in to that account, with the existing external login method, and set a password. 
            Then you can associate it with additional external login methods.";

        public const string ExistingDoubleUser = @"There is an existing account with same email, but is not the one which you are trying to link it with. 
            Either link it to other account, or delete other account and try linking it this one again.";


        public const string LoginAlreadyAssociatedWithAccount = "Login is already associated with existing account";

        public const string InvalidLogin = "Invalid login attempt.";

    }

    internal static class LogMessages 
    {
        public const string IsNull = "{0} is null";
        public const string InstanceCreation = "Can't create an instance of '{instance}'. Ensure that '{instance}' is not an abstract class and has a parameterless constructor.";

        public const string UserTriedDuplicatingAccount = "User from provider {0} with key {1}, tried to link the account with {2}, but there is already other user {3} with that address.";
        public const string UserCreatedWithPassword = "User {0} created a new account with password.";
        public const string UserCreatedWithProvider = "User {0} created an account using {1} provider.";
        public const string UserLogin = "User {0} logged in.";
        public const string UserLockedOut = "User {0} locked out.";
        public const string UserLoginInWithProvider = "User {Name} logged in with {LoginProvider} provider.";
        public const string EmailStoreNotSupported = "The default UI requires a user store with email support.";
        public const string ErrorFromExternalProvider = "Error from external provider: {0}";
        public const string ErrorLoadingExternalLogin = "Error loading external login information.";
    }
}
