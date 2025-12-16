namespace Sven.PageModels.Connect.ExternalProviders
{
    public static class ExternalProviders
    {
        public static readonly List<ExternalProvider> ExternalList =
        [
            new ExternalProvider()
            {
                ProviderCodeName = "google",
                IsSignInEnabled = true,
                ProviderName = "Google",
                IconCssClass = "bi-google",
            },
            new ExternalProvider()
            {
                ProviderCodeName = "microsoft",
                IsSignInEnabled = false,
                ProviderName = "Microsoft",
                IconCssClass = "bi-microsoft",
            },
        ];
    }
}
