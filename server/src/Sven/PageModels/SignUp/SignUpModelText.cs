namespace Sven.PageModels.SignUp
{
    public class SignUpModelText : SignModelText
    {
        public required string AlreadyAccount { get; init; }
        public required string Create { get; init; }
        public required string CreateAccountTitle { get; init; }
        public required string ConfirmPassword { get; init; }
        public required string Email { get; init; }
        public required string SignIn { get; init; }
        public required string SignUpWith { get; init; }

    }

    public class SignModelText
    {
        public required string Or { get; init; }

        public required string Password { get; init; }

        public required string Username { get; init; }
    }
}
