using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public SignInModel(IPageModelFactory<SignInPageModel> modelFactory)
        {
            _modelFactory = modelFactory;
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
}
