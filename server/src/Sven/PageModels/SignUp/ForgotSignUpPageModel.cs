
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
        private readonly ISymEncryptor _encryptor;
        private readonly INotificationService<PasswordResetNotification> _notificationService;
        public override bool ShowExternalLoginsOption { get { return false; } }
        public override bool ShowSignInOption { get { return false; } }

        public ForgotSignUpPageModel(ILogger<ForgotSignUpPageModel> logger,
            IStringLocalizer<SignUpModel> localizer,
            IUserProvider userProvider,
            ISymEncryptor encryptor,
            INotificationService<PasswordResetNotification> notificationService) : base(logger, localizer, userProvider)
        {
            Title = _localizer[nameof(SignUpModelText.ForgotPasswordTitle)];
            Subtitle = _localizer[nameof(SignUpModelText.ForgotPasswordSubtitle)];
            Subtitle = _localizer[nameof(SignUpModelText.ForgotPasswordSubtitle)];
            FinalMessage = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessage)];
            FinalMessageLine1 = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessageLine1)];
            _encryptor = encryptor;
            _notificationService = notificationService;
        }

        protected override VerificationStatus StatusToValidateInSetPassword() => VerificationStatus.EmailSent;

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
                    return HandleGetWithEmail(stepChallenge);
                case SignUpStep.SetPassword:
                    return await HandleGetWithChallengeAsync(stepChallenge, cancellationToken);
                default:
                    return GoToUrlWithError(Endpoints.Connect.SignIn, new ResultError($"Unexpected step in {nameof(ForgotSignUpPageModel)}: {stepChallenge.Step}"));
            }
        }

        private async Task<IActionResult> HandleGetWithChallengeAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken, [CallerMemberName] string callerName = "")
        {
            Result<UserVerificationCode> codeResult = await VerifyChallengeStatusAsync(stepChallenge.Challenge!, VerificationStatus.EmailSent, cancellationToken, callerName);
            if (codeResult.IsError)
            {
                return GoToUrlWithError(Endpoints.Connect.SignIn, codeResult.Error);
            }
            Email = codeResult.Value.Email;
            return BasePage.Page();
        }

        private IActionResult HandleGetWithEmail(StepChallengeRequest stepChallenge)
        {
            if (string.IsNullOrWhiteSpace(stepChallenge.Challenge))
            {
                return GoToUrlWithError(Endpoints.Connect.SignIn, new ResultError("Challenge is required for CodeSent step."));
            }
            Result<string> emailResult = _encryptor.DecryptString(stepChallenge.Challenge);
            if (emailResult.IsError)
            {
                return GoToUrlWithError(Endpoints.Connect.SignIn, new ResultError(emailResult.Error, "Challenge is required for CodeSent step."));
            }
            if (SvenValidators.ValidateEmail(emailResult.Value) is { IsError: true } result)
            {
                return GoToUrlWithError(Endpoints.Connect.SignIn, new ResultError(result.Error, $"Challenge is required for CodeSent step."));
            }
            Email = emailResult.Value;
            return BasePage.Page();
        }

        public override async Task<IActionResult> HandlePostSetEmailAsync(CancellationToken cancellationToken = default)
        {
            Result<string> emailEncryptedResult = _encryptor.Encrypt(Email!);
            if (emailEncryptedResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(emailEncryptedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return await GenerateAndSendAsync(() => GoToSameRouteWithChallenge(new StepChallengeRequest(SignUpStep.CodeSent) { Challenge = emailEncryptedResult.Value }), cancellationToken);
        }

        private async Task<IActionResult> GenerateAndSendAsync(Func<IActionResult> returnFunc, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userProvider.GenerateVerificationCodeAsync(Email!, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (!codeResult.Value.IsUserKnown)
            {
                _logger.LogResultError(new ResultError($"User tried to reset password with non existing email ({Email}, {codeResult.Value})"));
                if (await _userProvider.UpdateVerificationCodeStatusAsync(codeResult.Value.Id, VerificationStatus.Invalid, cancellationToken) is { IsError: true } updateResult)
                {
                    _logger.LogResultError(new ResultError(updateResult.Error, $"Error when setting the verification status  ({Email}, {codeResult.Value})"));
                }
                return returnFunc();
            }
            if (await SendAndUpdateAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return returnFunc();
        }

        /// <summary>
        /// This is doing resending
        /// </summary>
        /// <param name="stepChallenge"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<IActionResult> HandlePostCodeSentAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            Result<string> emailResult = _encryptor.DecryptString(stepChallenge.Challenge!);
            if (emailResult.IsError || !emailResult.Value.Equals(Email))
            {
                return GoToUrlWithError(Endpoints.Connect.ForgotPassword, new ResultError("Unexpected behaviour: email not in challenge or not the same."));
            }
            return await GenerateAndSendAsync(() => BasePage.Page(), cancellationToken);
        }

        private async Task<Result> SendAndUpdateAsync(UserVerificationCode code, CancellationToken cancellationToken)
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
            if (await _userProvider.UpdateVerificationCodeStatusAsync(code.Id, VerificationStatus.EmailSent, cancellationToken) is { IsError: true } result)
            {
                return new ResultError(result.Error, $"Error when setting the verification status  ({Email}, {code})");
            }
            return notificationResult;
        }

        protected override async Task<IActionResult> HandlePostSetPasswordInternalAsync(IPasswordResetProperties model, UserVerificationCode verificationCode, CancellationToken cancellationToken = default)
        {
            if (!verificationCode.IsUserKnown || verificationCode.Status.HasFlag(VerificationStatus.Invalid))
            {
                return BasePage.PageWithError(new ResultError(AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            Result userCreatedResult = await _userProvider.UpdatePasswordAsync(verificationCode.UserId!, model.Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(userCreatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToSameRouteWithChallenge(new StepChallengeRequest(SignUpStep.FinalMessage) { Challenge = verificationCode.Id });
        }
    }
}
