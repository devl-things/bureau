namespace Sven.Configurations
{
    public static class ViewConstants
    {
        public static class PropertyNames
        {
            public const string Action = "action";
            public const string ConfirmPassword = "confirmPassword";
            public const string Email = "email";
            public const string Id = "id";
            public const string Password = "password";
            public const string Username = "username";
            public const string VerificationCode = "verificationCode";
            public const string Step = "s";
        }

        public static class Actions
        {
            public const string VerifyCode = "verify";
            public const string ResendCode = "resend";
        }
    }
}
