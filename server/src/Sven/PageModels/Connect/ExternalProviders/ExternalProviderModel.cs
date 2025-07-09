namespace Sven.PageModels.Connect.ExternalProviders
{
    public class ExternalProviderModel
    {
        public bool IsSignInEnabled { get; set; }
        public string SignInLink { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string IconCssClass { get; set; } = string.Empty;
    }
}
