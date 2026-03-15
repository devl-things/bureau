using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class PlainVerifyCodeStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<PlainVerifyCodeStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly INotificationService<UserVerificationCodeNotification> _notificationService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.VerifyCode; } }

        public PlainVerifyCodeStepHandler(
            ILogger<PlainVerifyCodeStepHandler> logger,
            IUserService userService,
            INotificationService<UserVerificationCodeNotification> notificationService,
            SignUpFlowHelper flowHelper,
            ErrorTranslations errorTranslations)
        {
            _logger = logger;
            _userService = userService;
            _notificationService = notificationService;
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
            context.ViewModel.Email = codeResult.Value.Email;
            return context.Page();
        }

        public async Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidateEmail(context.StepModel!.Email) is { IsError: true } emailValidationResult)
            {
                return context.PageWithError(emailValidationResult.Error);
            }
            string email = context.StepModel!.Email!;
            context.ViewModel.Email = email;

            if (await _userService.ExistsUserWithEmailAsync(email, cancellationToken))
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, ResultError.From(string.Format(ErrorMessages.EmailExisting, email), _errorTranslations.SignUpExistingUser));
            }
            Result<UserVerificationCode> verifyCodeResult = await _userService.GetVerificationCodeAsync(context.StepChallenge.Challenge!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignUp, ResultError.From(verifyCodeResult.Error, "Verification code not created."));
            }
            if (!verifyCodeResult.Value.Email.Equals(email, StringComparison.OrdinalIgnoreCase) || verifyCodeResult.Value.IsUserKnown)
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, ResultError.From("Unexpected behaviour: either incorrect email or known user."));
            }
            string? action = context.Request.GetFormStringParameter(ViewConstants.PropertyNames.Action);
            if (string.Equals(action, ViewConstants.Actions.ResendCode, StringComparison.OrdinalIgnoreCase))
            {
                return await ResendActionAsync(context, verifyCodeResult.Value, cancellationToken);
            }
            return await VerifyActionAsync(context, verifyCodeResult.Value, cancellationToken);
        }

        private async Task<IActionResult> VerifyActionAsync(SignUpStepContext context, UserVerificationCode verificationCode, CancellationToken cancellationToken)
        {
#if DEBUG
            if ("000000".Equals(context.StepModel!.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                StepChallengeRequest debugStep = new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = verificationCode.Id };
                return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, debugStep));
            }
#endif
            if (string.IsNullOrWhiteSpace(context.StepModel!.VerificationCode)
                || !int.TryParse(context.StepModel.VerificationCode, out int parsedCode)
                || parsedCode < _userService.MinVerificationCode
                || parsedCode > _userService.MaxVerificationCode
                || !context.StepModel.VerificationCode.Equals(verificationCode.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return context.PageWithError(ResultError.From(_errorTranslations.InvalidVerificationCode));
            }
            if (await _userService.UpdateVerificationCodeStatusAsync(verificationCode.Id, VerificationStatus.Verified, cancellationToken) is { IsError: true } codeValidatedResult)
            {
                return context.PageWithError(ResultError.From(codeValidatedResult.Error, _errorTranslations.UnManageable));
            }
            StepChallengeRequest nextStep = new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = verificationCode.Id };
            return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, nextStep));
        }

        private async Task<IActionResult> ResendActionAsync(SignUpStepContext context, UserVerificationCode verificationCode, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userService.RegenerateVerificationCodeAsync(verificationCode, cancellationToken);
            if (codeResult.IsError)
            {
                return context.PageWithError(ResultError.From(codeResult.Error, _errorTranslations.UnManageable));
            }
            UserVerificationCodeNotification notification = new UserVerificationCodeNotification
            {
                Code = codeResult.Value.VerificationCode,
                Email = codeResult.Value.Email
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return context.PageWithError(ResultError.From(notificationResult.Error, _errorTranslations.UnManageable));
            }
            if (await _userService.UpdateVerificationCodeStatusAsync(codeResult.Value.Id, VerificationStatus.EmailSent, cancellationToken) is { IsError: true } result)
            {
                return context.PageWithError(ResultError.From(result.Error, _errorTranslations.UnManageable));
            }
            StepChallengeRequest nextStep = new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id };
            return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, nextStep));
        }
    }
}
