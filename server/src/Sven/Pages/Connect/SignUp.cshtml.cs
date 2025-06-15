using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.SignUp;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignUpModel : PageModel
    {
        private readonly ILogger<SignUpModel> _logger;
        private readonly IPageModelFactory<SignUpModel, SignUpPageModel> _modelFactory;

        public SignUpModelText Text { get; init; }
        public SignUpPageModel CurrentModel { get; protected set; } = null!;
        public string? ErrorMessage { get; set; }

        public SignUpModel(ILogger<SignUpModel> logger,
            IStringLocalizer<Resources.Common> localizer,
            IStringLocalizer<Resources.Pages.Connect> signUpLocalizer,
            IPageModelFactory<SignUpModel, SignUpPageModel> modelFactory) : base()
        {
            _logger = logger;
            _modelFactory = modelFactory;
            Text = new SignUpModelText()
            {
                AlreadyAccount = signUpLocalizer[Resources.Pages.Connect.MsgAlreadyAccount],
                Create = signUpLocalizer[Resources.Pages.Connect.BtnCreate],
                CodeSentMessage = signUpLocalizer[Resources.Pages.Connect.CodeSent_MsgMain],
                CodeSentMessageLine1 = signUpLocalizer[Resources.Pages.Connect.CodeSent_MsgLine1],
                CodeSentMessageLine2 = signUpLocalizer[Resources.Pages.Connect.CodeSent_MsgLine2],
                ConfirmPassword = localizer[Resources.Common.LblConfirmPassword],
                Continue = localizer[Resources.Common.BtnContinue],
                Email = localizer[Resources.Common.LblEmail],
                NeedNewCode = signUpLocalizer[Resources.Pages.Connect.MsgNeedNewCode],
                Or = localizer[Resources.Common.Or],
                Password = localizer[Resources.Common.LblPassword],
                ResendCode = signUpLocalizer[Resources.Pages.Connect.BtnResendCode],
                ResendResetLink = signUpLocalizer[Resources.Pages.Connect.BtnResendResetLink],
                SetPasswordMessage = signUpLocalizer[Resources.Pages.Connect.SetPassword_Msg],
                SignIn = signUpLocalizer[Resources.Pages.Connect.BtnLogin],
                SignUpWith = signUpLocalizer[Resources.Pages.Connect.MsgSignUpWith],
                VerifyCode = signUpLocalizer[Resources.Pages.Connect.BtnVerifyCode],
                VerifyCodeMessage = signUpLocalizer[Resources.Pages.Connect.VerifyCode_Msg],

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
