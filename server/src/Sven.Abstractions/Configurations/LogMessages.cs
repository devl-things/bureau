namespace Sven.Configurations
{
    public static class LogMessages
    {
        public const string InvalidChallengeForStep = "User tried to open {0} step with wrong challenge ({1})";
        public const string UnsupportedModality1 = "Something called {0} in not supported mode, with {1}";
        public const string UnsupportedModality2 = "Something called {0} in not supported mode, with {1} and {2}";
    }

    public static class ErrorMessages
    {
        public const string EmailEmpty = "Email cannot be empty.";
        public const string PasswordEmpty = "Password cannot be empty.";
    }
}
