namespace Sven.PageModels.SignUp
{
    public class SignUpModelText : SignModelText
    {
        public required string AlreadyAccount { get; init; }
        public required string Create { get; init; }
        public required string CodeSentMessage { get; init; }
        public required string CodeSentMessageLine1 { get; init; }
        public required string CodeSentMessageLine2 { get; init; }
        public required string ConfirmPassword { get; init; }
        public required string Continue { get; init; }
        public required string Email { get; init; }
        public required string NeedNewCode { get; init; }
        public required string ResendCode { get; init; }
        public required string ResendResetLink { get; init; }
        public required string SignIn { get; init; }
        public required string SetPasswordMessage { get; init; }
        public required string SignUpWith { get; init; }
        public required string VerifyCode { get; init; }
        public required string VerifyCodeMessage { get; init; }


        public string? CreateAccountTitle { get; init; }
        public string? ForgotPasswordTitle { get; init; }
        public string? ForgotPasswordSubtitle { get; init; }
        public string? ForgotPasswordFinalMessage { get; init; }
        public string? ForgotPasswordFinalMessageLine1 { get; init; }

    }

    public class SignModelText
    {
        public required string Or { get; init; }

        public required string Password { get; init; }

    }
}
