using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels.ExternalLogins;
using Sven.Pages.Connect;
using Sven.Services;
using System.Runtime.CompilerServices;

namespace Sven.PageModels.SignUp
{
    public abstract class SignUpPageModel : IExternalLoginProperty
    {
        protected readonly ILogger<SignUpPageModel> _logger;
        protected readonly IStringLocalizer<SignUpModel> _localizer;
        protected readonly IUserProvider _userProvider;

        public SignUpModel BasePage { get; set; } = null!;
        public SignUpStep Step { get; set; }
        public string? Email { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }

        public string? FinalMessage { get; set; }
        public string? FinalMessageLine1 { get; set; }
        public virtual bool ShowExternalLoginsOption { get; }
        public virtual bool ShowSignInOption { get; }
        public List<ExternalLoginModel> ExternalLogins { get; init; }

        protected SignUpPageModel(ILogger<SignUpPageModel> logger, IStringLocalizer<SignUpModel> localizer, IUserProvider userProvider)
        {
            _logger = logger;
            _localizer = localizer;
            _userProvider = userProvider;
            ExternalLogins = ExternalLoginProviders.ExternalList;
        }

        internal Task<IActionResult> HandleGetRequestAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default)
        {
            Step = stepChallenge.Step;
            return HandleGetRequestInternalAsync(stepChallenge, cancellationToken);
        }

        protected abstract Task<IActionResult> HandleGetRequestInternalAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken);

        protected IActionResult HandleUnallowed(StepChallengeRequest stepChallenge, [CallerMemberName] string callerName = "")
        {
            return HandleUnallowed(new ResultError(string.Format(LogMessages.UnsupportedModality1, callerName, stepChallenge)));
        }

        protected IActionResult HandleUnallowed(StepChallengeRequest stepChallenge, object model, [CallerMemberName] string callerName = "")
        {
            return HandleUnallowed(new ResultError(string.Format(LogMessages.UnsupportedModality2, callerName, stepChallenge, model)));
        }

        protected IActionResult HandleUnallowed(ResultError error, [CallerMemberName] string callerName = "")
        {
            _logger.LogResultError(error);
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }

        internal async Task<IActionResult> HandlePostRequestAsync(StepChallengeRequest stepChallenge, StepModelRequest model, CancellationToken cancellationToken)
        {
            if (stepChallenge.Step != model.Step)
            {
                _logger.LogResultError(new ResultError($"Step mismatch: {stepChallenge.Step} != {model.Step}"));
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            Step = stepChallenge.Step;
            if (SignUpStep.FinalMessage.Equals(Step))
            {
                return HandleUnallowed(stepChallenge, model, nameof(HandlePostRequestAsync));
            }
            return await HandlePostStepsAsync(stepChallenge, model, cancellationToken);

        }

        private async Task<IActionResult> HandlePostStepsAsync(StepChallengeRequest stepChallenge, StepModelRequest model, CancellationToken cancellationToken)
        {
            if (SvenValidators.ValidateEmail(model.Email) is { IsError: true } emailValidationResult)
            {
                return BasePage.PageWithError(emailValidationResult.Error);
            }
            Email = model.Email;
            return stepChallenge.Step switch
            {
                SignUpStep.SetPassword => await HandlePostSetPasswordAsync(stepChallenge, model, cancellationToken),
                SignUpStep.VerifyCode => await HandlePostVerifyCodeAsync(stepChallenge, model, cancellationToken),
                SignUpStep.CodeSent => await HandlePostCodeSentAsync(stepChallenge, model, cancellationToken),
                _ => await HandlePostSetEmailAsync(cancellationToken), // this is EnterEmail too
            };
        }

        public abstract Task<IActionResult> HandlePostSetEmailAsync(CancellationToken cancellationToken = default);

        public async Task<IActionResult> HandlePostSetPasswordAsync(StepChallengeRequest stepChallenge, IPasswordResetProperties model, CancellationToken cancellationToken = default)
        {
            if (SvenValidators.ValidatePasswords(model) is { IsError: true } validationResult)
            {
                return BasePage.PageWithError(validationResult.Error);
            }
            Result<UserVerificationCode> codeResult = await VerifyChallengeStatusAsync(stepChallenge.Challenge!, StatusToValidateInSetPassword(), cancellationToken);
            if (codeResult.IsError)
            {
                return GoToUrlWithError(Endpoints.Connect.SignIn, codeResult.Error);
            }
            return await HandlePostSetPasswordInternalAsync(model, codeResult.Value, cancellationToken);
        }
        protected abstract Task<IActionResult> HandlePostSetPasswordInternalAsync(IPasswordResetProperties model, UserVerificationCode verificationCode, CancellationToken cancellationToken = default);

        protected abstract VerificationStatus StatusToValidateInSetPassword();

        public virtual Task<IActionResult> HandlePostVerifyCodeAsync(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HandleUnallowed(stepChallenge, model, nameof(HandlePostVerifyCodeAsync)));
        }

        public virtual Task<IActionResult> HandlePostCodeSentAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HandleUnallowed(stepChallenge, model, nameof(HandlePostCodeSentAsync)));
        }

        protected async Task<Result<UserVerificationCode>> VerifyChallengeStatusAsync(string challenge, VerificationStatus status, CancellationToken cancellationToken, [CallerMemberName] string callerName = "")
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

        public static string CreateUrl(string url, StepChallengeRequest stepChallenge)
        {
            if (string.IsNullOrEmpty(stepChallenge.Challenge))
            {
                return $"{url}?{AuthConstants.PropertyNames.Step}={stepChallenge.StepShort}";
            }
            return $"{url}?{AuthConstants.PropertyNames.Step}={stepChallenge.StepShort}&{AuthConstants.PropertyNames.Challenge}={stepChallenge.Challenge}";
        }

        protected IActionResult GoToSameRouteWithChallenge(StepChallengeRequest stepChallenge)
        {
            return BasePage.GoToUrl(CreateUrl(BasePage.Request.Path, stepChallenge));
        }
        protected IActionResult GoToUrlWithError(string url, ResultError error)
        {
            // #54 have a error message pass to redirect
            _logger.LogResultError(error);
            return BasePage.GoToUrl(url);
        }
    }
}
