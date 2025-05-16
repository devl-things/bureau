using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.SignIn;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignInModel : LocalizationPageModel<SignInModel, SignInModelText>
    {
        private readonly IPageModelFactory<SignInPageModel> _modelFactory;

        private readonly SignInModelText _text;

        public SignInModel(IStringLocalizer<Common> sharedLocalizer, IStringLocalizer<SignInModel> localizer,
            IPageModelFactory<SignInPageModel> modelFactory) : base(sharedLocalizer, localizer)
        {
            _modelFactory = modelFactory;
            _text = new SignInModelText()
            {
                ForgotPassword = _localizer[nameof(SignInModelText.ForgotPassword)],
                Login = _localizer[nameof(SignInModelText.Login)],
                Or = _sharedLocalizer[nameof(SignInModelText.Or)],
                Password = _sharedLocalizer[nameof(SignInModelText.Password)],
                SignInWith = _localizer[nameof(SignInModelText.SignInWith)],
                SignUp = _localizer[nameof(SignInModelText.SignUp)],
                Username = _sharedLocalizer[nameof(SignInModelText.Username)],
                Welcome = _localizer[nameof(SignInModelText.Welcome)]
            };
        }
        public SignInPageModel CurrentModel { get; protected set; } = null!;

        public override SignInModelText Text { get { return _text; } }

        public IActionResult OnGet([FromQuery(Name = AuthConstants.OAuth.FieldNames.Mode)] string? mode)
        {
            SetCurrentModel(mode);
            return CurrentModel.HandleGetRequest();
        }

        public Task<IActionResult> OnPostLoginAsync([FromForm] LoginCredentials credentials, string? mode, CancellationToken cancellationToken = default)
        {
            SetCurrentModel(mode);
            return CurrentModel.HandleLoginAsync(credentials, cancellationToken);
        }

        private void SetCurrentModel(string? mode)
        {
            CurrentModel = _modelFactory.CreateModel(mode, PageContext);
        }

    }
}
