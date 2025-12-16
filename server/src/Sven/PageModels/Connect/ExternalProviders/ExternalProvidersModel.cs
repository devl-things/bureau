using Sven.Configurations;

namespace Sven.PageModels.Connect.ExternalProviders
{
    public class ExternalProvidersModel
    {
        private string _mode;
        public IReadOnlyList<ExternalProvider> Providers { get; }
        public string Mode
        {
            get { return _mode; }
        }

        public ExternalProvidersModel()
        {
            Providers = ExternalProviders.ExternalList;
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
        /// Code name used in links or schemas, convention all small letters.
        /// </summary>
        public string ProviderCodeName { get; set; } = string.Empty;
        public bool IsSignInEnabled { get; set; }

        public string GetSignInLink(string? mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return $"{Endpoints.External.SignIn}/{ProviderCodeName}";
            }
            return $"{Endpoints.External.SignIn}/{ProviderCodeName}?{AuthConstants.PropertyNames.Mode}={mode}";
        }
        public string ProviderName { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string IconCssClass { get; set; } = string.Empty;

    }
}
