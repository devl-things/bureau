using Sven.Configurations;

namespace Sven.PageModels
{
    public interface IExternalLoginProperty
    {
        public List<ExternalLoginPageModel> ExternalLogins { get; init; }
    }

    public static class ExternalLoginProviders
    {
        public static readonly List<ExternalLoginPageModel> ExternalList =
        [
            new ExternalLoginPageModel()
            {
                IsSignInEnabled = true,
                SignInLink = Endpoints.External.SignInGoogle,
                ProviderName = "Google"
            },
            new ExternalLoginPageModel()
            {
                IsSignInEnabled = false,
                SignInLink = Endpoints.External.SignInMicrosoft,
                ProviderName = "Microsoft"
            },
        ];
    }
}
