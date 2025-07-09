using Microsoft.Extensions.Localization;

namespace Sven.PageModels.Connect
{
    public class ConnectTranslations : ISignInTranslations
    {
        public required string AlreadyAccount { get; init; }
        public required string Create { get; init; }
        public string? CreateAccountTitle { get; init; }
        public required string CodeSentMessage { get; init; }
        public required string CodeSentMessageLine1 { get; init; }
        public required string CodeSentMessageLine2 { get; init; }
        public required string ConfirmPassword { get; init; }
        public required string Continue { get; init; }
        public required string Email { get; init; }
        public required string ForgotPassword { get; init; }
        public required string LblForgotSignUpTitle { get; init; }
        public required string LblForgotSignUpSubtitle { get; init; }
        public required string LblPlainSignUpTitle { get; init; }
        public required string Login { get; init; }
        public required string MsgForgotSignUpFinalMessageTitle { get; init; }
        public required string MsgForgotSignUpFinalMessageLine1 { get; init; }
        public required string NeedNewCode { get; init; }
        public required string Or { get; init; }
        public required string Password { get; init; }
        public required string ResendCode { get; init; }
        public required string ResendResetLink { get; init; }
        public required string SetPasswordMessage { get; init; }
        public required string SignIn { get; init; }
        public required string SignInWith { get; init; }
        public required string SignUp { get; init; }
        public required string SignUpWith { get; init; }
        public required string UsernameOrEmail { get; init; }
        public required string VerifyCode { get; init; }
        public required string VerifyCodeMessage { get; init; }
        public required string Welcome { get; init; }

        public ConnectTranslations(IStringLocalizer<Resources.Common> localizer, IStringLocalizer<Resources.Pages.Connect> connectLocalizer)
        {
            ForgotPassword = connectLocalizer[Resources.Pages.Connect.MsgForgotPassword];
            Login = connectLocalizer[Resources.Pages.Connect.BtnLogin];
            Or = localizer[Resources.Common.Or];
            Password = localizer[Resources.Common.LblPassword];
            SignInWith = connectLocalizer[Resources.Pages.Connect.MsgSignInWith];
            SignUp = connectLocalizer[Resources.Pages.Connect.BtnSignUp];
            UsernameOrEmail = localizer[Resources.Common.LblUsernameOrEmail];
            Welcome = connectLocalizer[Resources.Pages.Connect.MsgWelcome];

            AlreadyAccount = connectLocalizer[Resources.Pages.Connect.MsgAlreadyAccount];
            Create = connectLocalizer[Resources.Pages.Connect.BtnCreate];
            CodeSentMessage = connectLocalizer[Resources.Pages.Connect.CodeSent_MsgMain];
            CodeSentMessageLine1 = connectLocalizer[Resources.Pages.Connect.CodeSent_MsgLine1];
            CodeSentMessageLine2 = connectLocalizer[Resources.Pages.Connect.CodeSent_MsgLine2];
            ConfirmPassword = localizer[Resources.Common.LblConfirmPassword];
            Continue = localizer[Resources.Common.BtnContinue];
            Email = localizer[Resources.Common.LblEmail];
            NeedNewCode = connectLocalizer[Resources.Pages.Connect.MsgNeedNewCode];
            Or = localizer[Resources.Common.Or];
            Password = localizer[Resources.Common.LblPassword];
            ResendCode = connectLocalizer[Resources.Pages.Connect.BtnResendCode];
            ResendResetLink = connectLocalizer[Resources.Pages.Connect.BtnResendResetLink];
            SetPasswordMessage = connectLocalizer[Resources.Pages.Connect.SetPassword_Msg];
            SignIn = connectLocalizer[Resources.Pages.Connect.BtnLogin];
            SignUpWith = connectLocalizer[Resources.Pages.Connect.MsgSignUpWith];
            VerifyCode = connectLocalizer[Resources.Pages.Connect.BtnVerifyCode];
            VerifyCodeMessage = connectLocalizer[Resources.Pages.Connect.VerifyCode_Msg];

            LblForgotSignUpTitle = connectLocalizer[Resources.Pages.Connect.ForgotSignUp_Title];
            LblForgotSignUpSubtitle = connectLocalizer[Resources.Pages.Connect.ForgotSignUp_Subtitle];
            LblPlainSignUpTitle = connectLocalizer[Resources.Pages.Connect.PlainSignUp_Title];
            MsgForgotSignUpFinalMessageTitle = connectLocalizer[Resources.Pages.Connect.ForgotSignUp_MsgFinal];
            MsgForgotSignUpFinalMessageLine1 = connectLocalizer[Resources.Pages.Connect.ForgotSignUp_MsgLine1];
        }
    }

    public interface ISignTranslations
    {
        public string Or { get; }

        public string Password { get; }

    }

    public class ExternalProvidersTranslations
    {
        public string PrefixText { get; init; }
        public ExternalProvidersTranslations(string prefixText)
        {
            PrefixText = prefixText;
        }
    }

    public interface ISignInTranslations : ISignTranslations
    {
        public string ForgotPassword { get; }
        public string Login { get; }
        public string SignInWith { get; }
        public string SignUp { get; }
        public string UsernameOrEmail { get; }
        public string Welcome { get; }
    }

    public class SignUpTranslations
    {
        public required string Title { get; init; }
        public string? Subtitle { get; init; }

        public required string FinalMessage { get; init; }
        public required string FinalMessageLine1 { get; init; }
    }

    public interface ISignUpTranslations : ISignTranslations
    {
        public string AlreadyAccount { get; }
        public string Create { get; }
        public string CodeSentMessage { get; }
        public string CodeSentMessageLine1 { get; }
        public string CodeSentMessageLine2 { get; }
        public string ConfirmPassword { get; }
        public string Continue { get; }
        public string Email { get; }
        public string NeedNewCode { get; }
        public string ResendCode { get; }
        public string ResendResetLink { get; }
        public string SignIn { get; }
        public string SetPasswordMessage { get; }
        public string SignUpWith { get; }
        public string VerifyCode { get; }
        public string VerifyCodeMessage { get; }


        public string? CreateAccountTitle { get; }
        public string? ForgotPasswordTitle { get; }
        public string? ForgotPasswordSubtitle { get; }
        public string? ForgotPasswordFinalMessage { get; }
        public string? ForgotPasswordFinalMessageLine1 { get; }

    }
}
