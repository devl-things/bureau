using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.SignUp;
using Sven.Services;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignUpModel : LocalizationPageModel<SignUpModel, SignUpModelText>
    {
        private readonly ILogger<SignUpModel> _logger;
        private readonly IPageModelFactory<SignUpModel, SignUpPageModel> _modelFactory;

        private readonly SignUpModelText _text;

        public override SignUpModelText Text { get { return _text; } }

        public SignUpPageModel CurrentModel { get; protected set; } = null!;
        public string? ErrorMessage { get; set; }
        public SignUpModel(ILogger<SignUpModel> logger, ISymEncryptor encryptor,
            IStringLocalizer<Common> stringLocalizer,
            IStringLocalizer<SignUpModel> localizer,
            IPageModelFactory<SignUpModel, SignUpPageModel> modelFactory) : base(stringLocalizer, localizer)
        {
            _logger = logger;
            _modelFactory = modelFactory;
            _text = new SignUpModelText()
            {
                AlreadyAccount = _localizer[nameof(SignUpModelText.AlreadyAccount)],
                Create = _localizer[nameof(SignUpModelText.Create)],
                CodeSentMessage = _localizer[nameof(SignUpModelText.CodeSentMessage)],
                CodeSentMessageLine1 = _localizer[nameof(SignUpModelText.CodeSentMessageLine1)],
                CodeSentMessageLine2 = _localizer[nameof(SignUpModelText.CodeSentMessageLine2)],
                ConfirmPassword = _sharedLocalizer[nameof(SignUpModelText.ConfirmPassword)],
                Continue = _localizer[nameof(SignUpModelText.Continue)],
                Email = _sharedLocalizer[nameof(SignUpModelText.Email)],
                NeedNewCode = _localizer[nameof(SignUpModelText.NeedNewCode)],
                Or = _sharedLocalizer[nameof(SignUpModelText.Or)],
                Password = _sharedLocalizer[nameof(SignUpModelText.Password)],
                ResendCode = _localizer[nameof(SignUpModelText.ResendCode)],
                ResendResetLink = _localizer[nameof(SignUpModelText.ResendResetLink)],
                SetPasswordMessage = _localizer[nameof(SignUpModelText.SetPasswordMessage)],
                SignIn = _localizer[nameof(SignUpModelText.SignIn)],
                SignUpWith = _localizer[nameof(SignUpModelText.SignUpWith)],
                VerifyCode = _localizer[nameof(SignUpModelText.VerifyCode)],
                VerifyCodeMessage = _localizer[nameof(SignUpModelText.VerifyCodeMessage)],

            };
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
            ErrorMessage = error.UserMessage;
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
