using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class ForgotSetPasswordStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<ForgotSetPasswordStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.SetPassword; } }

        public ForgotSetPasswordStepHandler(
            ILogger<ForgotSetPasswordStepHandler> logger,
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
            Result<UserVerificationCode> codeResult = await _flowHelper.VerifyChallengeStatusAsync(context.StepChallenge.Challenge!, VerificationStatus.EmailSent, cancellationToken);
            if (codeResult.IsError)
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, codeResult.Error);
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
            Result<UserVerificationCode> codeResult = await _flowHelper.VerifyChallengeStatusAsync(context.StepChallenge.Challenge!, VerificationStatus.EmailSent, cancellationToken);
            if (codeResult.IsError)
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, codeResult.Error);
            }
            if (!codeResult.Value.IsUserKnown || codeResult.Value.Status.HasFlag(VerificationStatus.Invalid))
            {
                return context.PageWithError(ResultError.From(_errorTranslations.UnManageable));
            }
            context.ViewModel.Email = codeResult.Value.Email;
            Result userUpdatedResult = await _userService.UpdatePasswordAsync(codeResult.Value.UserId!, context.StepModel!.Password!, cancellationToken);
            if (userUpdatedResult.IsError)
            {
                return context.PageWithError(ResultError.From(userUpdatedResult.Error, _errorTranslations.UnManageable));
            }
            StepChallengeRequest nextStep = new StepChallengeRequest(SignUpStep.FinalMessage) { Challenge = codeResult.Value.Id };
            return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, nextStep));
        }
    }
}
