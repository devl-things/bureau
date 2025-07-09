using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.PageModels.Account.ChangePassword;
using Sven.Services;

namespace Sven.Pages.Account
{
    public class ChangePasswordModel : AuthPageModel
    {
        private readonly IUserProvider _userProvider;

        [BindProperty]
        public PasswordChangeModel Input { get; set; } = new();

        public IPasswordChangeTranslations T9n { get; init; }

        public ChangePasswordModel(ICurrentUserProvider currentUserProvider,
            AccountTranslations translations, IUserProvider userProvider) : base(currentUserProvider)
        {
            _userProvider = userProvider;
            T9n = translations;
        }
        public IActionResult OnGet()
        {

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (!PasswordHasher.VerifyPassword(CurrentUser.PasswordHash, Input.CurrentPassword!))
            {
                ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.CurrentPassword)}", T9n.MsgInputIsInvalid);
                return Page();
            }

            // #54
            Result result = await _userProvider.UpdatePasswordAsync(CurrentUser.SubjectId, Input.Password!, cancellationToken);

            TempData[TempDataNames.SuccessMessage] = T9n.MsgChangePasswordSuccess;
            return LocalRedirect(Endpoints.Account.AccountInfo);
        }
    }
}
