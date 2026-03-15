using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class PlainEnterEmailStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<PlainEnterEmailStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly INotificationService<UserVerificationCodeNotification> _notificationService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.EnterEmail; } }

        public PlainEnterEmailStepHandler(
            ILogger<PlainEnterEmailStepHandler> logger,
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

        public Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            context.ViewModel.ShowExternalLoginsOption = true;
            context.ViewModel.ShowSignInOption = true;
            return Task.FromResult<IActionResult>(context.Page());
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
                return context.PageWithError(ResultError.From(string.Format(ErrorMessages.EmailExisting, email), _errorTranslations.SignUpExistingUser));
            }
            Result<UserVerificationCode> codeResult = await _userService.GenerateVerificationCodeAsync(email, cancellationToken);
            if (codeResult.IsError)
            {
                return context.PageWithError(ResultError.From(codeResult.Error, _errorTranslations.UnManageable));
            }
            if (await SendAndUpdateAsync(codeResult.Value, email, cancellationToken) is { IsError: true } codeSentResult)
            {
                return context.PageWithError(ResultError.From(codeSentResult.Error, _errorTranslations.UnManageable));
            }
            StepChallengeRequest nextStep = new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id };
            return context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, nextStep));
        }

        private async Task<Result> SendAndUpdateAsync(UserVerificationCode code, string email, CancellationToken cancellationToken)
        {
            UserVerificationCodeNotification notification = new UserVerificationCodeNotification
            {
                Code = code.VerificationCode,
                Email = code.Email
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return ResultError.From(notificationResult.Error, "Email cannot be sent.");
            }
            if (await _userService.UpdateVerificationCodeStatusAsync(code.Id, VerificationStatus.EmailSent, cancellationToken) is { IsError: true } result)
            {
                return ResultError.From(result.Error, $"Error when setting the verification status ({email}, {code})");
            }
            return notificationResult;
        }
    }
}
