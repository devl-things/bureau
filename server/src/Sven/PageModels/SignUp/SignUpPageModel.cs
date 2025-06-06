using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels.ExternalLogins;
using Sven.Pages.Connect;
using Sven.Services;

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

        internal async Task<IActionResult> HandleGetRequestAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken)
        {
            Step = stepChallenge.Step;
            switch (stepChallenge.Step)
            {
                case SignUpStep.VerifyCode:
                    return await HandleGetVerifyCodeAsync(stepChallenge, cancellationToken);
                case SignUpStep.CodeSent:
                    return await HandleGetCodeSentAsync(stepChallenge, cancellationToken);
                case SignUpStep.SetPassword:
                    return await HandleGetSetPasswordAsync(stepChallenge, cancellationToken);
                case SignUpStep.FinalMessage:
                    return await HandleGetFinalMessageAsync(stepChallenge, cancellationToken);
                case SignUpStep.EnterEmail:
                default:
                    return await HandleGetEnterEmailAsync(stepChallenge, cancellationToken);
            }
        }

        protected virtual Task<IActionResult> HandleGetVerifyCodeAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandleGetVerifyCodeAsync)} in not supported mode, with {stepChallenge}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }
        protected virtual Task<IActionResult> HandleGetCodeSentAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandleGetCodeSentAsync)} in not supported mode, with {stepChallenge}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }
        protected abstract Task<IActionResult> HandleGetSetPasswordAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default);
        protected virtual Task<IActionResult> HandleGetFinalMessageAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandleGetFinalMessageAsync)} in not supported mode, with {stepChallenge}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }
        protected virtual Task<IActionResult> HandleGetEnterEmailAsync(StepChallengeRequest stepChallenge, CancellationToken cancellationToken = default)
        {
            Step = stepChallenge.Step;
            return Task.FromResult<IActionResult>(BasePage.Page());
        }
        internal async Task<IActionResult> HandlePostRequestAsync(StepChallengeRequest stepChallenge, StepModelRequest model, CancellationToken cancellationToken)
        {
            Step = stepChallenge.Step;
            return stepChallenge.Step switch
            {
                SignUpStep.SetPassword => await HandlePostSetPasswordAsync(stepChallenge, model, cancellationToken),
                SignUpStep.VerifyCode => await HandlePostVerifyCodeAsync(stepChallenge, model, cancellationToken),
                SignUpStep.CodeSent => await HandlePostCodeSentAsync(stepChallenge, model, cancellationToken),
                SignUpStep.FinalMessage => HandlePostFinalMessage(stepChallenge, model),
                _ => await HandlePostSetEmailAsync(stepChallenge, model, cancellationToken), // this is EnterEmail too
            };
        }
        private IActionResult HandlePostFinalMessage(StepChallengeRequest stepChallenge, StepModelRequest model)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandlePostVerifyCodeAsync)} in not supported mode, with {stepChallenge} and {model}"));
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);
        }
        public abstract Task<IActionResult> HandlePostSetEmailAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default);

        public abstract Task<IActionResult> HandlePostSetPasswordAsync(StepChallengeRequest stepChallenge, IPasswordResetProperties model, CancellationToken cancellationToken = default);

        public virtual Task<IActionResult> HandlePostVerifyCodeAsync(StepChallengeRequest stepChallenge, IVerificationCodeProperties model, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandlePostVerifyCodeAsync)} in not supported mode, with  {stepChallenge} and {model}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }

        public virtual Task<IActionResult> HandlePostCodeSentAsync(StepChallengeRequest stepChallenge, IStepEmailProperties model, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError($"Something called {nameof(HandlePostCodeSentAsync)} in not supported mode, with  {stepChallenge} and {model}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }

        public static string CreateUrl(string url, StepChallengeRequest stepChallenge)
        {
            return $"{url}?{AuthConstants.PropertyNames.Step}={stepChallenge.StepShort}&{AuthConstants.PropertyNames.Challenge}={stepChallenge.Challenge}";
        }

        public IActionResult GoToWithChallenge(string url, StepChallengeRequest stepChallenge)
        {
            return BasePage.GoToUrl(CreateUrl(url, stepChallenge));
        }

    }
}
