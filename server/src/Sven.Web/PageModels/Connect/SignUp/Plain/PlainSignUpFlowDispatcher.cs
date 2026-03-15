using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class PlainSignUpFlowDispatcher : ISignUpFlowDispatcher
    {
        private readonly ILogger<PlainSignUpFlowDispatcher> _logger;
        private readonly PlainEnterEmailStepHandler _enterEmailHandler;
        private readonly PlainVerifyCodeStepHandler _verifyCodeHandler;
        private readonly PlainSetPasswordStepHandler _setPasswordHandler;
        private readonly PlainFinalMessageStepHandler _finalMessageHandler;
        private readonly ConnectTranslations _translations;
        private readonly IExternalProviderRegistry _registry;

        public PlainSignUpFlowDispatcher(
            ILogger<PlainSignUpFlowDispatcher> logger,
            PlainEnterEmailStepHandler enterEmailHandler,
            PlainVerifyCodeStepHandler verifyCodeHandler,
            PlainSetPasswordStepHandler setPasswordHandler,
            PlainFinalMessageStepHandler finalMessageHandler,
            ConnectTranslations translations,
            IExternalProviderRegistry registry)
        {
            _logger = logger;
            _enterEmailHandler = enterEmailHandler;
            _verifyCodeHandler = verifyCodeHandler;
            _setPasswordHandler = setPasswordHandler;
            _finalMessageHandler = finalMessageHandler;
            _translations = translations;
            _registry = registry;
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
                Title = _translations.LblPlainSignUpTitle,
                FinalMessage = _translations.MsgForgotSignUpFinalMessageTitle,
                FinalMessageLine1 = _translations.MsgForgotSignUpFinalMessageLine1
            };
            context.ViewModel.ExternalProvidersViewModel = new ExternalProvidersViewModel(_registry)
            {
                T9n = new ExternalProvidersTranslations(_translations.SignUpWith)
            };
        }

        private ISignUpStepHandler GetHandler(SignUpStep step)
        {
            return step switch
            {
                SignUpStep.VerifyCode => _verifyCodeHandler,
                SignUpStep.SetPassword => _setPasswordHandler,
                SignUpStep.FinalMessage => _finalMessageHandler,
                _ => _enterEmailHandler
            };
        }
    }
}
