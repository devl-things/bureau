namespace Sven.PageModels.ExternalLogins
{
    public class ExternalLoginModel
    {
        public bool IsSignInEnabled { get; set; }
        public string SignInLink { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
    }
}
