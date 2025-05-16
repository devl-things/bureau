using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.PageModels;
using Sven.PageModels.SignIn;
using Sven.PageModels.SignUp;

namespace Sven.Pages.Connect
{
    public class SignUpModel : LocalizationPageModel<SignUpModel, SignUpModelText>, IExternalLoginProperty
    {
        private readonly SignUpModelText _text;
        public SignUpModel(IStringLocalizer<Common> stringLocalizer, IStringLocalizer<SignUpModel> localizer) : base(stringLocalizer, localizer)
        {
            _text = new SignUpModelText()
            {
                AlreadyAccount = _localizer[nameof(SignUpModelText.AlreadyAccount)],
                Create = _localizer[nameof(SignUpModelText.Create)],
                CreateAccountTitle = _localizer[nameof(SignUpModelText.CreateAccountTitle)],
                ConfirmPassword = _sharedLocalizer[nameof(SignUpModelText.ConfirmPassword)],
                Email = _sharedLocalizer[nameof(SignUpModelText.Email)],
                Or = _sharedLocalizer[nameof(SignInModelText.Or)],
                Password = _sharedLocalizer[nameof(SignInModelText.Password)],
                SignIn = _localizer[nameof(SignUpModelText.CreateAccountTitle)],
                SignUpWith = _localizer[nameof(SignUpModelText.SignUpWith)],
                Username = _sharedLocalizer[nameof(SignInModelText.Username)],
            };
            ExternalLogins = ExternalLoginProviders.ExternalList;
        }
        public override SignUpModelText Text { get { return _text; } }

        public List<ExternalLoginPageModel> ExternalLogins { get; init; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public Task<IActionResult> OnPostSignUpAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IActionResult)Page());
        }
    }
}
