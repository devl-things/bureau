using Bureau;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.SignUp;
using Sven.Services;
using Sven.Extensions;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignUpModel : PageModel
    {
        private readonly ILogger<SignUpModel> _logger;
        private readonly PlainSignUpFlowDispatcher _plainDispatcher;
        private readonly ForgotSignUpFlowDispatcher _forgotDispatcher;

        public ConnectTranslations T9n { get; init; }
        public SignUpViewModel CurrentModel { get; private set; }
        public string? ErrorMessage { get; set; }

        public SignUpModel(
            ILogger<SignUpModel> logger,
            ConnectTranslations translations,
            PlainSignUpFlowDispatcher plainDispatcher,
            ForgotSignUpFlowDispatcher forgotDispatcher)
        {
            _logger = logger;
            _plainDispatcher = plainDispatcher;
            _forgotDispatcher = forgotDispatcher;
            T9n = translations;
            CurrentModel = new SignUpViewModel();
        }

        public Task<IActionResult> OnGetAsync([FromQuery] StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            ISignUpFlowDispatcher dispatcher = GetDispatcher();
            SignUpStepContext context = CreateContext(stepChallenge);
            return dispatcher.HandleGetAsync(context, cancellationToken);
        }

        public Task<IActionResult> OnPostAsync([FromQuery] StepChallengeRequest stepChallenge, [FromForm] StepModelRequest model, CancellationToken cancellationToken = default)
        {
            ISignUpFlowDispatcher dispatcher = GetDispatcher();
            SignUpStepContext context = CreateContext(stepChallenge, model);
            return dispatcher.HandlePostAsync(context, cancellationToken);
        }

        private ISignUpFlowDispatcher GetDispatcher()
        {
            if (string.Equals(Request.Path.Value, Endpoints.Connect.ForgotPassword, StringComparison.OrdinalIgnoreCase))
            {
                return _forgotDispatcher;
            }
            return _plainDispatcher;
        }

        private SignUpStepContext CreateContext(StepChallengeRequest stepChallenge, StepModelRequest? stepModel = null)
        {
            return new SignUpStepContext(
                stepChallenge: stepChallenge,
                stepModel: stepModel,
                viewModel: CurrentModel,
                pageFactory: () => Page(),
                pageWithErrorFactory: (error) =>
                {
                    _logger.LogResultError(error);
                    ErrorMessage = error.ErrorMessage;
                    return Page();
                },
                redirectFactory: (url) => Redirect(url),
                request: Request);
        }
    }
}
