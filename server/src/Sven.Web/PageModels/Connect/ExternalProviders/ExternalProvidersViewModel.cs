using Sven.Services;

namespace Sven.PageModels.Connect.ExternalProviders
{
    public class ExternalProvidersViewModel
    {
        public ExternalProvidersModel Data { get; set; }
        public required ExternalProvidersTranslations T9n { get; set; }

        public ExternalProvidersViewModel(IExternalProviderRegistry registry)
        {
            Data = new ExternalProvidersModel(registry);
        }
    }
}
