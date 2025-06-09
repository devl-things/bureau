
using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Models;
using Sven.Pages.Connect;
using Sven.Services;
using System.Runtime.CompilerServices;

namespace Sven.PageModels.SignUp
{
    public class ForgotSignUpPageModel : SignUpPageModel
    {
        private readonly INotificationService<PasswordResetNotification> _notificationService;
        public override bool ShowExternalLoginsOption { get { return false; } }
        public override bool ShowSignInOption { get { return false; } }

        public ForgotSignUpPageModel(ILogger<ForgotSignUpPageModel> logger,
            IStringLocalizer<SignUpModel> localizer,
            IUserProvider userProvider,
            INotificationService<PasswordResetNotification> notificationService) : base(logger, localizer, userProvider)
        {
            Title = _localizer[nameof(SignUpModelText.ForgotPasswordTitle)];
            Subtitle = _localizer[nameof(SignUpModelText.ForgotPasswordSubtitle)];
            Subtitle = _localizer[nameof(SignUpModelText.ForgotPasswordSubtitle)];
            FinalMessage = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessage)];
            FinalMessageLine1 = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessageLine1)];
            _notificationService = notificationService;
        }

        protected override async Task<IActionResult> HandleGetRequestInternalAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            switch (stepChallenge.Step)
            {
                case SignUpStep.EnterEmail:
                case SignUpStep.FinalMessage:
                    return BasePage.Page();
                case SignUpStep.VerifyCode:
                    return HandleUnallowed(stepChallenge);
                case SignUpStep.CodeSent:
                case SignUpStep.SetPassword:
                    return await HandleGetWithChallengeAsync(stepChallenge, cancellationToken);
                default:
                    _logger.LogResultError(new ResultError($"Unexpected step in {nameof(ForgotSignUpPageModel)}: {stepChallenge.Step}"));
                    return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
        }

        private async Task<IActionResult> HandleGetWithChallengeAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken, [CallerMemberName] string callerName = "")
        {
            Result<UserVerificationCode> codeResult = await VerifyChallengeStatusAsync(stepChallenge.Challenge!, VerificationStatus.EmailSent, cancellationToken, callerName);
            if (codeResult.IsError)
            {
                _logger.LogResultError(codeResult.Error);
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            Email = codeResult.Value.Email;
            return BasePage.Page();
        }

        protected async Task<Result<UserVerificationCode>> VerifyChallengeStatusAsync(string challenge, VerificationStatus status, CancellationToken cancellationToken, string callerName)
        {
            Result<UserVerificationCode> codeResult = await _userProvider.GetVerificationCodeAsync(challenge, cancellationToken);
            if (codeResult.IsError)
            {
                return new ResultError(codeResult.Error, string.Format(LogMessages.InvalidChallengeForStep, callerName, challenge));
            }
            if (!codeResult.Value.Status.HasFlag(status))
            {
                return new ResultError($"Verification status is not good ({codeResult.Value.Status})");
            }
            return codeResult.Value;
        }

        public override async Task<IActionResult> HandlePostSetEmailAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidateEmail(model.Email) is { IsError: true } emailValidationResult)
            {
                return BasePage.PageWithError(emailValidationResult.Error);
            }
            Email = model.Email;

            Result<UserVerificationCode> codeResult = await _userProvider.GenerateVerificationCodeAsync(Email!, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (!await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                _logger.LogResultError(new ResultError($"User tried to reset password with non existing email ({Email}, {codeResult.Value})"));
                codeResult.Value.Status = VerificationStatus.Invalid;
                if (await _userProvider.UpdateVerificationCodeAsync(codeResult.Value, cancellationToken) is { IsError: true } updateResult)
                {
                    // #54 have a error message pass to redirect
                    _logger.LogResultError(new ResultError(updateResult.Error, $"Error when setting the verification status  ({Email}, {codeResult.Value})"));
                }
                return GoToWithChallenge(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.CodeSent) { Challenge = codeResult.Value.Id });
            }
            if (await SendVerificationEmailAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            codeResult.Value.Status = VerificationStatus.EmailSent;
            if (await _userProvider.UpdateVerificationCodeAsync(codeResult.Value, cancellationToken) is { IsError: true } result)
            {
                // #54 have a error message pass to redirect
                _logger.LogResultError(new ResultError(result.Error, $"Error when setting the verification status  ({Email}, {codeResult.Value})"));
            }
            return GoToWithChallenge(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.CodeSent) { Challenge = codeResult.Value.Id });
        }

        public override async Task<IActionResult> HandlePostCodeSentAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            Result<UserVerificationCode> verifyCodeResult = await _userProvider.GetVerificationCodeAsync(stepChallenge.Challenge!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                _logger.LogResultError(new ResultError(verifyCodeResult.Error, "Verification code not created."));
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            if (verifyCodeResult.Value.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogResultError(new ResultError("Incorrect email."));
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            Result<UserVerificationCode> codeResult = await _userProvider.GenerateVerificationCodeAsync(verifyCodeResult.Value.Email, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (await SendVerificationEmailAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToWithChallenge(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
        }

        public override async Task<IActionResult> HandlePostSetPasswordAsync(StepChallengeRequest stepChallenge, IPasswordResetProperties model, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidateEmail(model.Email) is { IsError: true } emailValidationResult)
            {
                return BasePage.PageWithError(emailValidationResult.Error);
            }
            if (SvenValidators.ValidatePasswords(model) is { IsError: true } validationResult)
            {
                return BasePage.PageWithError(validationResult.Error);
            }
            Result<UserVerificationCode> codeResult = await VerifyChallengeStatusAsync(stepChallenge.Challenge!, VerificationStatus.EmailSent, cancellationToken, nameof(HandlePostSetPasswordAsync));
            if (codeResult.IsError)
            {
                _logger.LogResultError(codeResult.Error);
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            Email = model.Email;

            Result<string> userCreatedResult = await _userProvider.CreateUserAsync(Email!, model.Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(userCreatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToWithChallenge(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.FinalMessage) { Challenge = codeResult.Value.Id });
        }

        private async Task<Result> SendVerificationEmailAsync(UserVerificationCode code, CancellationToken cancellationToken)
        {
            PasswordResetNotification notification = new()
            {
                ResetLink = CreateUrl(Endpoints.Connect.ForgotPassword, new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = code.Id }),
                Email = code.Email
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return new ResultError(notificationResult.Error, "Email cannot be sent.");
            }
            return notificationResult;
        }
    }
}
