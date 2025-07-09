using Sven.Configurations;

namespace Sven.PageModels.Connect.ExternalProviders
{
    public static class ExternalProviders
    {
        public static readonly List<ExternalProviderModel> ExternalList =
        [
            new ExternalProviderModel()
            {
                IsSignInEnabled = true,
                SignInLink = Endpoints.External.SignInGoogle,
                ProviderName = "Google",
                IconCssClass = "bi-google",
            },
            new ExternalProviderModel()
            {
                IsSignInEnabled = false,
                SignInLink = Endpoints.External.SignInMicrosoft,
                ProviderName = "Microsoft",
                IconCssClass = "bi-microsoft",
            },
        ];
    }
}
