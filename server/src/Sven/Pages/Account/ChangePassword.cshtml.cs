using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.PageModels;
using Sven.PageModels.ChangePassword;
using Sven.Services;

namespace Sven.Pages.Account
{
    public class ChangePasswordModel : AuthPageModel
    {
        private readonly IStringLocalizer<Resources.Common> _localizer;
        private readonly IStringLocalizer<Resources.Pages.Account> _accountLocalizer;
        private readonly IUserProvider _userProvider;

        [BindProperty]
        public PasswordChangeModel Input { get; set; } = new();

        public PasswordChangeModelText Text { get; init; }

        public ChangePasswordModel(ICurrentUserProvider currentUserProvider,
            IStringLocalizer<Resources.Common> localizer, IStringLocalizer<Resources.Pages.Account> accountLocalizer, IUserProvider userProvider) : base(currentUserProvider)
        {
            _localizer = localizer;
            _accountLocalizer = accountLocalizer;
            _userProvider = userProvider;
            Text = new PasswordChangeModelText()
            {
                Title = _accountLocalizer[Resources.Pages.Account.ChangePassword_Title],
                BtnCancel = _localizer[Resources.Common.BtnCancel],
                BtnUpdate = _localizer[Resources.Common.BtnUpdate],
            };
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
                ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.CurrentPassword)}", _localizer[Resources.Common.MsgInputIsInvalid]);
                return Page();
            }

            // #54
            Result result = await _userProvider.UpdatePasswordAsync(CurrentUser.SubjectId, Input.Password!, cancellationToken);

            TempData[TempDataNames.SuccessMessage] = _accountLocalizer[Resources.Pages.Account.ChangePassword_MsgSuccess].ToString();
            return LocalRedirect(Endpoints.Account.AccountInfo);
        }
    }
}
