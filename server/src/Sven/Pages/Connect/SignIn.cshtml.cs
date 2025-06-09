using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        private readonly IPageModelFactory<PageContext, SignInPageModel> _modelFactory;

        private readonly SignInModelText _text;

        public SignInModel(IStringLocalizer<Common> sharedLocalizer, IStringLocalizer<SignInModel> localizer,
            IPageModelFactory<PageContext, SignInPageModel> modelFactory) : base(sharedLocalizer, localizer)
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
                UsernameOrEmail = _sharedLocalizer[nameof(SignInModelText.UsernameOrEmail)],
                Welcome = _localizer[nameof(SignInModelText.Welcome)]
            };
        }
        public SignInPageModel CurrentModel { get; protected set; } = null!;

        public override SignInModelText Text { get { return _text; } }

        public Task<IActionResult> OnGetAsync([FromQuery(Name = AuthConstants.PropertyNames.Mode)] string? mode, CancellationToken cancellationToken = default)
        {
            SetCurrentModel(mode);
            return CurrentModel.HandleGetRequestAsync(cancellationToken);
        }

        public Task<IActionResult> OnPostLoginAsync([FromForm] LoginCredentialsRequest credentials, [FromForm(Name = AuthConstants.PropertyNames.Mode)] string? mode, CancellationToken cancellationToken = default)
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
