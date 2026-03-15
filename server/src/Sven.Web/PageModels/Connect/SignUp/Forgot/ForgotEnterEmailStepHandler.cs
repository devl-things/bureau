using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class ForgotEnterEmailStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<ForgotEnterEmailStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly ISymEncryptor _encryptor;
        private readonly INotificationService<PasswordResetNotification> _notificationService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.EnterEmail; } }

        public ForgotEnterEmailStepHandler(
            ILogger<ForgotEnterEmailStepHandler> logger,
            IUserService userService,
            ISymEncryptor encryptor,
            INotificationService<PasswordResetNotification> notificationService,
            SignUpFlowHelper flowHelper,
            ErrorTranslations errorTranslations)
        {
            _logger = logger;
            _userService = userService;
            _encryptor = encryptor;
            _notificationService = notificationService;
            _flowHelper = flowHelper;
            _errorTranslations = errorTranslations;
        }

        public Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
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

            Result<string> emailEncryptedResult = _encryptor.Encrypt(email);
            if (emailEncryptedResult.IsError)
            {
                return context.PageWithError(ResultError.From(emailEncryptedResult.Error, _errorTranslations.UnManageable));
            }
            return await GenerateAndSendAsync(
                context,
                email,
                () => context.GoToUrl(SignUpFlowHelper.BuildStepUrl(context.Request.Path, new StepChallengeRequest(SignUpStep.CodeSent) { Challenge = emailEncryptedResult.Value })),
                cancellationToken);
        }

        private async Task<IActionResult> GenerateAndSendAsync(SignUpStepContext context, string email, Func<IActionResult> returnFunc, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userService.GenerateVerificationCodeAsync(email, cancellationToken);
            if (codeResult.IsError)
            {
                return context.PageWithError(ResultError.From(codeResult.Error, _errorTranslations.UnManageable));
            }
            if (!codeResult.Value.IsUserKnown)
            {
                _logger.LogResultError(ResultError.From($"User tried to reset password with non existing email ({email}, {codeResult.Value})"));
                if (await _userService.UpdateVerificationCodeStatusAsync(codeResult.Value.Id, VerificationStatus.Invalid, cancellationToken) is { IsError: true } updateResult)
                {
                    _logger.LogResultError(ResultError.From(updateResult.Error, $"Error when setting the verification status ({email}, {codeResult.Value})"));
                }
                return returnFunc();
            }
            PasswordResetNotification notification = new PasswordResetNotification
            {
                ResetLink = SignUpFlowHelper.BuildStepUrl(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = codeResult.Value.Id }),
                Email = codeResult.Value.Email
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return context.PageWithError(ResultError.From(notificationResult.Error, _errorTranslations.UnManageable));
            }
            if (await _userService.UpdateVerificationCodeStatusAsync(codeResult.Value.Id, VerificationStatus.EmailSent, cancellationToken) is { IsError: true } result)
            {
                return context.PageWithError(ResultError.From(result.Error, $"Error when setting the verification status ({email}, {codeResult.Value})"));
            }
            return returnFunc();
        }
    }
}
