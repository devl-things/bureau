using Sven.PageModels.Connect.ExternalProviders;

namespace Sven.PageModels.Account.LinkedIdentities
{
    public class LinkedIdentitiesViewModel
    {
        public LinkedIdentitiesViewModelData Data { get; set; } = new();
        public LinkedIdentityViewOptions Options { get; set; } = new();

        public required ILinkedIdentityTranslations T9n { get; set; }

    }

    public class LinkedIdentitiesViewModelData
    {
        public List<LinkedIdentityModel> LinkedIdentities { get; set; } = [];
        public List<ExternalProviderModel> ExternalProviders { get; set; } = [];
    }
}
