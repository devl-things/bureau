namespace Bureau.Identity.UI.Constants
{
    internal static class BureauIdentityUIMessages
    {

        public const string AccountDoesntExist = "Such account doesn't exist. Either omit password and create new one, or insert correct username/password combination";

        public const string ExistingUserByWrongPassword = @"There is an existing account, however, that account has no password set. 
            Please log in to that account, with the existing external login method, and set a password. 
            Then you can associate it with additional external login methods.";

        public const string ExistingDoubleUser = @"There is an existing account with same email, but is not the one which you are trying to link it with. 
            Either link it to other account, or delete other account and try linking it this one again.";

        public const string InvalidLogin = "Invalid login attempt.";

        public const string ConfirmEmailChange = "Thank you for confirming your email change.";
    }
}
