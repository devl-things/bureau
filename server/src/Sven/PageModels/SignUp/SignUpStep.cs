namespace Sven.PageModels.SignUp
{
    public enum SignUpStep
    {
        EnterEmail,
        VerifyCode,
        CodeSent,
        SetPassword,
        FinalMessage
    }

    public static class SignUpStepExtensions
    {
        public static string ToCode(this SignUpStep step) => step switch
        {
            SignUpStep.EnterEmail => "ee",
            SignUpStep.VerifyCode => "vc",
            SignUpStep.CodeSent => "cs",
            SignUpStep.SetPassword => "sp",
            SignUpStep.FinalMessage => "fm",
            _ => "ee"
        };

        public static SignUpStep ToSignUpStep(string code) => code.ToLowerInvariant() switch
        {
            "ee" => SignUpStep.EnterEmail,
            "vc" => SignUpStep.VerifyCode,
            "cs" => SignUpStep.CodeSent,
            "sp" => SignUpStep.SetPassword,
            "fm" => SignUpStep.FinalMessage,
            _ => SignUpStep.EnterEmail
        };

        public static bool TryToSignUpStep(string code, out SignUpStep step)
        {
            switch (code.ToLowerInvariant())
            {
                case "ee": step = SignUpStep.EnterEmail; return true;
                case "vc": step = SignUpStep.VerifyCode; return true;
                case "cs": step = SignUpStep.CodeSent; return true;
                case "sp": step = SignUpStep.SetPassword; return true;
                case "fm": step = SignUpStep.FinalMessage; return true;
                default: step = default; return false;
            }
        }
    }
}
