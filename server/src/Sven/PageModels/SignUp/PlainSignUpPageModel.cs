
using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Pages.Connect;
using Sven.Services;

namespace Sven.PageModels.SignUp
{
    public class PlainSignUpPageModel : SignUpPageModel
    {

        private readonly INotificationService<UserVerificationCodeNotification> _notificationService;

        public override bool ShowExternalLoginsOption { get { return Step == SignUpStep.EnterEmail; } }

        public override bool ShowSignInOption { get { return Step == SignUpStep.EnterEmail; } }
        public PlainSignUpPageModel(ILogger<PlainSignUpPageModel> logger, IStringLocalizer<SignUpModel> localizer, IUserProvider userProvider,
            INotificationService<UserVerificationCodeNotification> notificationService) : base(logger, localizer, userProvider)
        {
            _notificationService = notificationService;
            Title = _localizer[nameof(SignUpModelText.CreateAccountTitle)];
            FinalMessage = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessage)];
            FinalMessageLine1 = _localizer[nameof(SignUpModelText.ForgotPasswordFinalMessageLine1)];
        }
        protected override VerificationStatus StatusToValidateInSetPassword() => VerificationStatus.Verified;

        protected override async Task<IActionResult> HandleGetRequestInternalAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            switch (stepChallenge.Step)
            {
                case SignUpStep.CodeSent:
                    return HandleUnallowed(stepChallenge);
                case SignUpStep.VerifyCode:
                case SignUpStep.SetPassword:
                    return await HandleGetStepsAsync(stepChallenge, cancellationToken);
                case SignUpStep.EnterEmail:
                case SignUpStep.FinalMessage:
                default:
                    return BasePage.Page();
            }
        }

        private async Task<IActionResult> HandleGetStepsAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userProvider.GetVerificationCodeAsync(stepChallenge.Challenge!, cancellationToken);
            if (codeResult.IsError)
            {
                _logger.LogResultError(new ResultError(codeResult.Error, string.Format(LogMessages.InvalidChallengeForStep, nameof(HandleGetStepsAsync), stepChallenge.Challenge)));
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            if (SignUpStep.SetPassword.Equals(Step) && !codeResult.Value.Status.HasFlag(VerificationStatus.Verified))
            {
                _logger.LogResultError(new ResultError(string.Format(LogMessages.InvalidChallengeForStep, nameof(HandleGetStepsAsync), stepChallenge.Challenge)));
                return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
            }
            Email = codeResult.Value.Email;
            return BasePage.Page();
        }

        public override async Task<IActionResult> HandlePostSetEmailAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            if (await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                // #54 have a error message pass to redirect
                _logger.LogResultError(new ResultError($"User tried to sign up with existing email ({Email})", AuthConstants.OAuth.ErrorDescriptions.SignUpExistingUser));
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            Result<UserVerificationCode> codeResult = await _userProvider.GenerateVerificationCodeAsync(Email!, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (await SendAndUpdateAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
        }

        public override async Task<IActionResult> HandlePostVerifyCodeAsync(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken = default)
        {
            if (await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                return BasePage.PageWithError(new ResultError($"User tried to sign up with existing email ({Email})", AuthConstants.OAuth.ErrorDescriptions.SignUpExistingUser));
            }
            Result<UserVerificationCode> verifyCodeResult = await _userProvider.GetVerificationCodeAsync(stepChallenge.Challenge!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                _logger.LogResultError(new ResultError(verifyCodeResult.Error, "Verification code not created."));
                return BasePage.GoToUrl(Endpoints.Connect.SignUp);
            }
            if (!verifyCodeResult.Value.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) ||
                verifyCodeResult.Value.IsUserKnown)
            {
                _logger.LogResultError(new ResultError("Unexpected behaviour: either incorrect email or known user."));
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);
            }
            string? action = BasePage.Request.GetFormStringParameter(AuthConstants.PropertyNames.Action);
            switch (action)
            {
                case AuthConstants.Actions.ResendCode:
                    return await ResendAction(verifyCodeResult.Value, cancellationToken);
                case AuthConstants.Actions.VerifyCode:
                default:
                    return await VerifyAction(verifyCodeResult.Value, model, cancellationToken);
            }
        }

        private async Task<IActionResult> VerifyAction(UserVerificationCode verificationCode, IVerificationCodeProperties model, CancellationToken cancellationToken)
        {
#if DEBUG
            if ("000000".Equals(model.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = verificationCode.Id });
            }
#endif
            if (string.IsNullOrWhiteSpace(model.VerificationCode) || !int.TryParse(model.VerificationCode, out int parsedCode)
                || parsedCode < _userProvider.MinVerificationCode || parsedCode > _userProvider.MaxVerificationCode
                || !model.VerificationCode.Equals(verificationCode.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return BasePage.PageWithError(new ResultError("Invalid verification code."));
            }
            if (await _userProvider.UpdateVerificationCodeStatusAsync(verificationCode.Id, VerificationStatus.Verified, cancellationToken) is { IsError: true } codeValidatedResult)
            {
                return BasePage.PageWithError(new ResultError(codeValidatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = verificationCode.Id });
        }

        private async Task<IActionResult> ResendAction(UserVerificationCode verificationCode, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userProvider.RegenerateVerificationCodeAsync(verificationCode, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (await SendAndUpdateAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
        }

        /// <summary>
        /// Send email and update status
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Result> SendAndUpdateAsync(UserVerificationCode code, CancellationToken cancellationToken)
        {
            UserVerificationCodeNotification notification = new()
            {
                Code = code.VerificationCode,
                Email = code.Email
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return new ResultError(notificationResult.Error, "Email cannot be sent.");
            }
            if (await _userProvider.UpdateVerificationCodeStatusAsync(code.Id, VerificationStatus.EmailSent, cancellationToken) is { IsError: true } result)
            {
                return new ResultError($"Error when setting the verification status  ({Email}, {code})");
            }
            return notificationResult;
        }

        protected override async Task<IActionResult> HandlePostSetPasswordInternalAsync(IPasswordResetProperties model, UserVerificationCode verificationCode, CancellationToken cancellationToken = default)
        {
            Result<string> userCreatedResult = await _userProvider.CreateUserAsync(Email!, model.Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(userCreatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToSamePageWithChallenge(new StepChallengeRequest(SignUpStep.FinalMessage) { Challenge = verificationCode.Id });
        }
    }
}
