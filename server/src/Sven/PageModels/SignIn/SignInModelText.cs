using Sven.PageModels.SignUp;

namespace Sven.PageModels.SignIn
{
    public class SignInModelText : SignModelText
    {
        public required string ForgotPassword { get; init; }
        public required string Login { get; init; }
        public required string SignInWith { get; init; }
        public required string SignUp { get; init; }
        public required string Welcome { get; init; }
    }
}
