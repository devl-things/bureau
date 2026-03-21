using Sven.Configurations;
using Sven.Services;

namespace Sven.PageModels.Connect.ExternalProviders
{
    public class ExternalProvidersModel
    {
        private string _mode;

        public IReadOnlyList<ExternalProvider> Providers { get; }

        public string Mode => _mode;

        public ExternalProvidersModel(IExternalProviderRegistry registry)
        {
            Providers = registry.Providers
                .Select(p => new ExternalProvider
                {
                    ProviderCodeName = p.ProviderKey,
                    ProviderName = p.DisplayName,
                    IconCssClass = p.IconCssClass,
                    CssClass = p.CssClass,
                    IsSignInEnabled = p.IsSignInEnabled
                })
                .ToList()
                .AsReadOnly();
            _mode = Modes.ExternalLogin.Plain;
        }

        public void SetMode(string mode)
        {
            _mode = mode;
        }
    }

    public class ExternalProvider
    {
        /// <summary>
        /// Code name used in links, convention all lowercase.
        /// </summary>
        public string ProviderCodeName { get; set; } = string.Empty;
        public bool IsSignInEnabled { get; set; }

        public string GetSignInLink(string? mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return $"{Endpoints.External.SignIn}/{ProviderCodeName}";
            return $"{Endpoints.External.SignIn}/{ProviderCodeName}?{AuthConstants.PropertyNames.Mode}={mode}";
        }

        public string ProviderName { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string IconCssClass { get; set; } = string.Empty;
    }
}
