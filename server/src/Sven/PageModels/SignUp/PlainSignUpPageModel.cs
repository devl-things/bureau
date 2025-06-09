
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
                return GoToWithChallenge(Endpoints.Connect.SignUp, new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
            }
            Email = codeResult.Value.Email;
            return BasePage.Page();
        }

        public override async Task<IActionResult> HandlePostSetEmailAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidateEmail(model.Email) is { IsError: true } emailValidationResult)
            {
                return BasePage.PageWithError(emailValidationResult.Error);
            }
            Email = model.Email;

            if (await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                return BasePage.PageWithError(new ResultError($"User tried to sign up with existing email ({Email})", AuthConstants.OAuth.ErrorDescriptions.SignUpExistingUser));
            }
            Result<UserVerificationCode> codeResult = await _userProvider.GenerateVerificationCodeAsync(Email!, cancellationToken);
            if (codeResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(codeResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            if (await SendVerificationEmailAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return BasePage.PageWithError(new ResultError(codeSentResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }
            return GoToWithChallenge(Endpoints.Connect.SignUp, new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
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
            Email = model.Email;

            Result<string> userCreatedResult = await _userProvider.CreateUserAsync(Email!, model.Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return BasePage.PageWithError(new ResultError(userCreatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable));
            }

            Result<string> ticketResult = await _userProvider.GenerateTicketAsync(userCreatedResult.Value, cancellationToken);
            if (userCreatedResult.IsError)
            {
                _logger.LogResultError(ticketResult.Error);
                return BasePage.GoToUrl(Endpoints.Connect.SignIn);

            }
            return BasePage.GoToUrl($"{Endpoints.Connect.SignIn}?{AuthConstants.PropertyNames.Mode}={PageModelTypes.SignIn.Ticket}&{AuthConstants.PropertyNames.Ticket}={ticketResult.Value}");

        }
        public override async Task<IActionResult> HandlePostVerifyCodeAsync(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidateEmail(model.Email) is { IsError: true } emailValidationResult)
            {
                return BasePage.PageWithError(emailValidationResult.Error);
            }
            Email = model.Email;
            if (await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                return BasePage.PageWithError(new ResultError($"User tried to sign up with existing email ({Email})", AuthConstants.OAuth.ErrorDescriptions.SignUpExistingUser));
            }
            string? action = BasePage.Request.GetFormStringParameter(AuthConstants.PropertyNames.Action);
            switch (action)
            {
                case AuthConstants.Actions.ResendCode:
                    return await ResendAction(stepChallenge, model, cancellationToken);
                case AuthConstants.Actions.VerifyCode:
                default:
                    return await VerifyAction(stepChallenge, model, cancellationToken);
            }
        }

        private async Task<IActionResult> VerifyAction(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken)
        {
            if (await VerifyCodeAsync(stepChallenge.Challenge, model, cancellationToken) is { IsError: true } codeVerifiedResult)
            {
                return BasePage.PageWithError(codeVerifiedResult.Error);
            }
            return GoToWithChallenge(Endpoints.Connect.SignUp, new StepChallengeRequest(SignUpStep.SetPassword) { Challenge = stepChallenge.Challenge });
        }

        private async Task<IActionResult> ResendAction(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> verifyCodeResult = await _userProvider.GetVerificationCodeAsync(stepChallenge.Challenge!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                _logger.LogResultError(new ResultError(verifyCodeResult.Error, "Verification code not created."));
                return BasePage.GoToUrl(Endpoints.Connect.SignUp);
            }
            if (!verifyCodeResult.Value.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogResultError(new ResultError("Incorrect email."));
                return BasePage.GoToUrl(Endpoints.Connect.SignUp);
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
            return GoToWithChallenge(Endpoints.Connect.SignUp, new StepChallengeRequest(SignUpStep.VerifyCode) { Challenge = codeResult.Value.Id });
        }

        private async Task<Result> VerifyCodeAsync(string? challenge, IVerificationCodeProperties model, CancellationToken cancellationToken)
        {
#if DEBUG
            if ("000000".Equals(model.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
#endif
            if (string.IsNullOrWhiteSpace(model.VerificationCode) || !int.TryParse(model.VerificationCode, out int parsedCode)
                || parsedCode < _userProvider.MinVerificationCode || parsedCode > _userProvider.MaxVerificationCode)
            {
                return new ResultError("Invalid verification code.");
            }

            Result<UserVerificationCode> verifyCodeResult = await _userProvider.GetVerificationCodeAsync(challenge!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                return new ResultError(verifyCodeResult.Error, "Verification code not created.");
            }
            if (!model.VerificationCode.Equals(verifyCodeResult.Value.VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultError("Incorrect verification code.");
            }
            if (!verifyCodeResult.Value.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultError("Incorrect email.");
            }
            verifyCodeResult.Value.Status = VerificationStatus.Verified;
            if (await _userProvider.UpdateVerificationCodeAsync(verifyCodeResult.Value, cancellationToken) is { IsError: true } codeValidatedResult)
            {
                return new ResultError(codeValidatedResult.Error, AuthConstants.OAuth.ErrorDescriptions.UnManageable);
            }
            return true;
        }

        private async Task<Result> SendVerificationEmailAsync(UserVerificationCode code, CancellationToken cancellationToken)
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
            return notificationResult;
        }
    }
}
