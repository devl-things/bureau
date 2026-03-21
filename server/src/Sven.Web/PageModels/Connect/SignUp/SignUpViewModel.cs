using Sven.PageModels.Connect.ExternalProviders;

namespace Sven.PageModels.Connect.SignUp
{
    public class SignUpViewModel
    {
        public SignUpStep Step { get; set; }
        public string? Email { get; set; }
        public SignUpTranslations T9n { get; set; } = null!;
        public bool ShowExternalLoginsOption { get; set; }
        public bool ShowSignInOption { get; set; }
        public ExternalProvidersViewModel ExternalProvidersViewModel { get; set; } = null!;
    }
}
