using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels.Connect;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class ForgotSignUpFlowDispatcher : ISignUpFlowDispatcher
    {
        private readonly ILogger<ForgotSignUpFlowDispatcher> _logger;
        private readonly ForgotEnterEmailStepHandler _enterEmailHandler;
        private readonly ForgotCodeSentStepHandler _codeSentHandler;
        private readonly ForgotSetPasswordStepHandler _setPasswordHandler;
        private readonly ForgotFinalMessageStepHandler _finalMessageHandler;
        private readonly ConnectTranslations _translations;

        public ForgotSignUpFlowDispatcher(
            ILogger<ForgotSignUpFlowDispatcher> logger,
            ForgotEnterEmailStepHandler enterEmailHandler,
            ForgotCodeSentStepHandler codeSentHandler,
            ForgotSetPasswordStepHandler setPasswordHandler,
            ForgotFinalMessageStepHandler finalMessageHandler,
            ConnectTranslations translations)
        {
            _logger = logger;
            _enterEmailHandler = enterEmailHandler;
            _codeSentHandler = codeSentHandler;
            _setPasswordHandler = setPasswordHandler;
            _finalMessageHandler = finalMessageHandler;
            _translations = translations;
        }

        public Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            InitViewModel(context);
            ISignUpStepHandler handler = GetHandler(context.StepChallenge.Step);
            context.ViewModel.Step = context.StepChallenge.Step;
            return handler.HandleGetAsync(context, cancellationToken);
        }

        public async Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            if (context.StepChallenge.Step != context.StepModel!.Step)
            {
                _logger.LogResultError(ResultError.From($"Step mismatch: {context.StepChallenge.Step} != {context.StepModel.Step}"));
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            InitViewModel(context);
            context.ViewModel.Step = context.StepChallenge.Step;
            ISignUpStepHandler handler = GetHandler(context.StepChallenge.Step);
            return await handler.HandlePostAsync(context, cancellationToken);
        }

        private void InitViewModel(SignUpStepContext context)
        {
            context.ViewModel.T9n = new SignUpTranslations
            {
                Title = _translations.LblForgotSignUpTitle,
                Subtitle = _translations.LblForgotSignUpSubtitle,
                FinalMessage = _translations.MsgForgotSignUpFinalMessageTitle,
                FinalMessageLine1 = _translations.MsgForgotSignUpFinalMessageLine1
            };
        }

        private ISignUpStepHandler GetHandler(SignUpStep step)
        {
            return step switch
            {
                SignUpStep.CodeSent => _codeSentHandler,
                SignUpStep.SetPassword => _setPasswordHandler,
                SignUpStep.FinalMessage => _finalMessageHandler,
                _ => _enterEmailHandler
            };
        }
    }
}
