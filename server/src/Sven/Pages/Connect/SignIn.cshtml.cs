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
        private readonly IPageModelFactory<PageContext, SignInPageModel> _modelFactory;

        public SignInPageModel CurrentModel { get; protected set; } = null!;
        public SignInModelText Text { get; init; }

        public SignInModel(IStringLocalizer<Resources.Common> localizer, IStringLocalizer<Resources.Pages.Connect> pageLocalizer,
            IPageModelFactory<PageContext, SignInPageModel> modelFactory) : base()
        {
            _modelFactory = modelFactory;
            Text = new SignInModelText()
            {
                ForgotPassword = pageLocalizer[Resources.Pages.Connect.MsgForgotPassword],
                Login = pageLocalizer[Resources.Pages.Connect.BtnLogin],
                Or = localizer[Resources.Common.Or],
                Password = localizer[Resources.Common.LblPassword],
                SignInWith = pageLocalizer[Resources.Pages.Connect.MsgSignInWith],
                SignUp = pageLocalizer[Resources.Pages.Connect.BtnSignUp],
                UsernameOrEmail = localizer[Resources.Common.LblUsernameOrEmail],
                Welcome = pageLocalizer[Resources.Pages.Connect.MsgWelcome]
            };
        }

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
