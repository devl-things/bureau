using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.PageModels.Account.LinkedIdentities;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.Services;

namespace Sven.Pages.Account
{
    public class LinkedIdentitiesModel : AuthPageModel
    {
        public LinkedIdentitiesViewModel LinkedIdentitiesViewModel { get; init; }
        public AccountTranslations T9n { get; init; }
        public LinkedIdentitiesModel(ICurrentUserProvider currentUserProvider,
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
                    ShowManageBtn = false,
                    ShowActions = true,
                    ShowAddOption = true
                },
                T9n = translations,
            };
        }

        public IActionResult OnGet()
        {
            LinkedIdentitiesViewModel.Data.LinkedIdentities = CurrentUser.LinkedIdentities?
                .Select(x => new LinkedIdentityModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProviderName = x.ProviderName,
                    Email = x.Email,
                    Status = x.Status,
                    IconClass = "bi-google",
                    AvailableActions = LinkedIdentityActions.Remove | (x.Status.HasValue && x.Status.Value ? LinkedIdentityActions.Disconnect : LinkedIdentityActions.Connect)
                }).ToList() ?? [];
            return Page();
        }

        public async Task<IActionResult> OnPostActionsAsync([FromForm(Name = ViewConstants.PropertyNames.Id)] string id, [FromForm(Name = ViewConstants.PropertyNames.Action)] LinkedIdentityActions action)
        {
            if (action.HasFlag(LinkedIdentityActions.Connect))
            {
                // Handle Connect
            }
            else if (action.HasFlag(LinkedIdentityActions.Disconnect))
            {
                // Handle Disconnect
            }
            else if (action.HasFlag(LinkedIdentityActions.Remove))
            {
                // Handle Remove
            }

            return RedirectToPage();
        }
    }
}
