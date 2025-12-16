using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.SignIn;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignInModel : PageModel
    {
        private readonly IPageModelFactory<PageContext, SignInPageModel> _modelFactory;

        public SignInPageModel CurrentModel { get; protected set; } = null!;
        public ISignInTranslations T9n { get; init; }

        public SignInModel(ConnectTranslations translations,
            IPageModelFactory<PageContext, SignInPageModel> modelFactory) : base()
        {
            _modelFactory = modelFactory;
            T9n = translations;
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
