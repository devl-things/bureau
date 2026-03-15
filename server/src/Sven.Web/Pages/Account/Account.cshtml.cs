using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.PageModels.Account.LinkedIdentities;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.Services;

namespace Sven.Pages.Account
{
    [Authorize]
    public class AccountModel : AuthPageModel
    {
        public LinkedIdentitiesViewModel LinkedIdentitiesViewModel { get; init; }
        public AccountTranslations T9n { get; init; }

        public AccountModel(ICurrentUserProvider currentUserProvider,
            AccountTranslations translations,
            IExternalProviderRegistry registry) : base(currentUserProvider)
        {
            T9n = translations;
            LinkedIdentitiesViewModel = new LinkedIdentitiesViewModel()
            {
                Data = new LinkedIdentitiesViewModelData()
                {
                    ExternalProviders = new ExternalProvidersModel(registry)
                },
                Options = new LinkedIdentityViewOptions()
                {
                    ShowManageBtn = true,
                    ShowActions = false,
                },
                T9n = translations,
            };
        }

        public IActionResult OnGet()
        {
            LinkedIdentitiesViewModel.Data.LinkedIdentities = CurrentUser?.LinkedIdentities?
                .Select(x => new LinkedIdentityModel()
                {
                    ProviderName = x.ProviderName,
                    Email = x.Email,
                    Status = x.Status,
                    IconClass = "bi-google",
                    AvailableActions = LinkedIdentityActions.Remove | (x.Status.HasValue && x.Status.Value ? LinkedIdentityActions.Disconnect : LinkedIdentityActions.Connect)
                }).ToList() ?? [];
            return Page();
        }
    }
}
