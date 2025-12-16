using Bureau;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Extensions;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.SignUp;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignUpModel : PageModel
    {
        private readonly ILogger<SignUpModel> _logger;
        private readonly IPageModelFactory<SignUpModel, SignUpPageModel> _modelFactory;

        public ConnectTranslations T9n { get; init; }
        public SignUpPageModel CurrentModel { get; protected set; } = null!;
        public string? ErrorMessage { get; set; }

        public SignUpModel(ILogger<SignUpModel> logger,
            ConnectTranslations translations,
            IPageModelFactory<SignUpModel, SignUpPageModel> modelFactory) : base()
        {
            _logger = logger;
            _modelFactory = modelFactory;
            T9n = translations;
        }


        public Task<IActionResult> OnGetAsync([FromQuery] StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            SetCurrentModel();
            return CurrentModel.HandleGetRequestAsync(stepChallenge, cancellationToken);
        }

        public Task<IActionResult> OnPostAsync([FromQuery] StepChallengeRequest stepChallenge, [FromForm] StepModelRequest model, CancellationToken cancellationToken = default)
        {
            SetCurrentModel();
            return CurrentModel.HandlePostRequestAsync(stepChallenge, model, cancellationToken);
        }

        public PageResult PageWithError(ResultError error)
        {
            _logger.LogResultError(error);
            //TODO #96 
            //ErrorMessage = error.UserMessage;
            ErrorMessage = error.ErrorMessage;
            return Page();
        }

        public IActionResult GoToUrl(string url)
        {
            return Redirect(url);
        }

        private void SetCurrentModel()
        {
            CurrentModel = _modelFactory.CreateModel(Request.Path, this);
        }
    }
}
