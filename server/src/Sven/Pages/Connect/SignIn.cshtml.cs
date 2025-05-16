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
    public class SignInModel : PageModel
    {
        private readonly IPageModelFactory<SignInPageModel> _modelFactory;

        #region text
        private readonly IStringLocalizer<SignInModel> _localizer;

        protected SignInModelText _text;
        protected SignInModelText Text
        {
            get { return _text; }
        }
        private SignInModelText SetText()
        {
            return new SignInModelText()
            {
                ContinueWith = _localizer[nameof(SignInModelText.ContinueWith)],
                ForgotPassword = _localizer[nameof(SignInModelText.ForgotPassword)],
                Login = _localizer[nameof(SignInModelText.Login)],
                Or = _localizer[nameof(SignInModelText.Or)],
                Password = _localizer[nameof(SignInModelText.Password)],
                SignUp = _localizer[nameof(SignInModelText.SignUp)],
                Username = _localizer[nameof(SignInModelText.Username)],
                Welcome = _localizer[nameof(SignInModelText.Welcome)]
            };
        }
        #endregion text

        public SignInModel(IStringLocalizer<SignInModel> localizer,
            IPageModelFactory<SignInPageModel> modelFactory)
        {
            _localizer = localizer;
            _modelFactory = modelFactory;
            _text = SetText();
        }
        public SignInPageModel CurrentModel { get; protected set; } = null!;
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

    public class SignInModelText
    {
        public required string ContinueWith { get; init; }
        public required string ForgotPassword { get; init; }
        public required string Login { get; init; }
        public required string Or { get; init; }
        public required string Password { get; init; }
        public required string SignUp { get; init; }
        public required string Username { get; init; }
        public required string Welcome { get; init; }
    }
}
