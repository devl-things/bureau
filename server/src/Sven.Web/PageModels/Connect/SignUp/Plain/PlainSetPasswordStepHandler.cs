using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class PlainSetPasswordStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<PlainSetPasswordStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.SetPassword; } }

        public PlainSetPasswordStepHandler(
            ILogger<PlainSetPasswordStepHandler> logger,
            IUserService userService,
            SignUpFlowHelper flowHelper,
            ErrorTranslations errorTranslations)
        {
            _logger = logger;
            _userService = userService;
            _flowHelper = flowHelper;
            _errorTranslations = errorTranslations;
        }

        public async Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            Result<UserVerificationCode> codeResult = await _userService.GetVerificationCodeAsync(context.StepChallenge.Challenge!, cancellationToken);
            if (codeResult.IsError)
            {
                _logger.LogResultError(ResultError.From(codeResult.Error, string.Format(LogMessages.InvalidChallengeForStep, nameof(HandleGetAsync), context.StepChallenge.Challenge)));
                return context.GoToUrl(Endpoints.Connect.SignIn);
            }
            if (!codeResult.Value.Status.HasFlag(VerificationStatus.Verified))
            {
                StepChallengeRequest verifyStep = new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id };
                return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, verifyStep));
            }
            context.ViewModel.Email = codeResult.Value.Email;
            return context.Page();
        }

        public async Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidatePasswords(context.StepModel!) is { IsError: true } validationResult)
            {
                return context.PageWithError(validationResult.Error);
            }
            Result<UserVerificationCode> codeResult = await _flowHelper.VerifyChallengeStatusAsync(context.StepChallenge.Challenge!, VerificationStatus.Verified, cancellationToken);
            if (codeResult.IsError)
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, codeResult.Error);
            }
            context.ViewModel.Email = codeResult.Value.Email;
            Result<string> userCreatedResult = await _userService.CreateUserAsync(codeResult.Value.Email, context.StepModel!.Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return context.PageWithError(ResultError.From(userCreatedResult.Error, _errorTranslations.UnManageable));
            }
            StepChallengeRequest nextStep = new StepChallengeRequest(SignUpStep.FinalMessage) { Challenge = codeResult.Value.Id };
            return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, nextStep));
        }
    }
}
