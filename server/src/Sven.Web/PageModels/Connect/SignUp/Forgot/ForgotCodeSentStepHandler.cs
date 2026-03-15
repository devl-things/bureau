using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven;
using Sven.Extensions;
using Sven.PageModels;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class ForgotCodeSentStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<ForgotCodeSentStepHandler> _logger;
        private readonly IUserService _userService;
        private readonly ISymEncryptor _encryptor;
        private readonly INotificationService<PasswordResetNotification> _notificationService;
        private readonly SignUpFlowHelper _flowHelper;
        private readonly ErrorTranslations _errorTranslations;

        public SignUpStep Step { get { return SignUpStep.CodeSent; } }

        public ForgotCodeSentStepHandler(
            ILogger<ForgotCodeSentStepHandler> logger,
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
            if (string.IsNullOrWhiteSpace(context.StepChallenge.Challenge))
            {
                return Task.FromResult(_flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, ResultError.From("Challenge is required for CodeSent step.")));
            }
            Result<string> emailResult = _encryptor.DecryptString(context.StepChallenge.Challenge);
            if (emailResult.IsError)
            {
                return Task.FromResult(_flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, ResultError.From(emailResult.Error, "Challenge is required for CodeSent step.")));
            }
            if (SvenValidators.ValidateEmail(emailResult.Value) is { IsError: true } result)
            {
                return Task.FromResult(_flowHelper.GoToUrlWithError(context, Endpoints.Connect.SignIn, ResultError.From(result.Error, "Challenge is required for CodeSent step.")));
            }
            context.ViewModel.Email = emailResult.Value;
            return Task.FromResult<IActionResult>(context.Page());
        }

        public async Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            Result<string> emailResult = _encryptor.DecryptString(context.StepChallenge.Challenge!);
            if (emailResult.IsError || !emailResult.Value.Equals(context.StepModel!.Email, StringComparison.OrdinalIgnoreCase))
            {
                return _flowHelper.GoToUrlWithError(context, Endpoints.Connect.ForgotPassword, ResultError.From("Unexpected behaviour: email not in challenge or not the same."));
            }
            string email = emailResult.Value;
            context.ViewModel.Email = email;
            return await GenerateAndSendAsync(context, email, () => context.Page(), cancellationToken);
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
