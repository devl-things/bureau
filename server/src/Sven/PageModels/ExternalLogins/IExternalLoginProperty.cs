using Sven.Configurations;

namespace Sven.PageModels.ExternalLogins
{
    public interface IExternalLoginProperty
    {
        public List<ExternalLoginModel> ExternalLogins { get; init; }
    }

    public static class ExternalLoginProviders
    {
        public static readonly List<ExternalLoginModel> ExternalList =
        [
            new ExternalLoginModel()
            {
                IsSignInEnabled = true,
                SignInLink = Endpoints.External.SignInGoogle,
                ProviderName = "Google"
            },
            new ExternalLoginModel()
            {
                IsSignInEnabled = false,
                SignInLink = Endpoints.External.SignInMicrosoft,
                ProviderName = "Microsoft"
            },
        ];
    }
}
