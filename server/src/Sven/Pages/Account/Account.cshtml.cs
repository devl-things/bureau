using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.Services;

namespace Sven.Pages.Account
{
    [Authorize]
    public class AccountModel : AuthPageModel
    {
        public AccountModelText Text { get; init; }

        public AccountModel(ICurrentUserProvider currentUserProvider,
            IStringLocalizer<Resources.Common> localizer, IStringLocalizer<Resources.Pages.Account> accountLocalizer) : base(currentUserProvider)
        {
            Text = new AccountModelText()
            {
                LblEmail = localizer[Resources.Common.LblEmail],
                LblPassword = localizer[Resources.Common.LblPassword],
                LblUsername = localizer[Resources.Common.LblUsername],
                Title = accountLocalizer[Resources.Pages.Account.Account_Title],
                BtnChangePassword = accountLocalizer[Resources.Pages.Account.BtnChangePassword],
            };
        }

        public IActionResult OnGet()
        {
            return Page();

        }
    }
}
