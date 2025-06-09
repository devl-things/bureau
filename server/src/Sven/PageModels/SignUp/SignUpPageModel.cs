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
            return HandleUnallowed(new ResultError(string.Format(LogMessages.UnsupportedModality, callerName, stepChallenge)));
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
            return stepChallenge.Step switch
            {
                SignUpStep.SetPassword => await HandlePostSetPasswordAsync(stepChallenge, model, cancellationToken),
                SignUpStep.VerifyCode => await HandlePostVerifyCodeAsync(stepChallenge, model, cancellationToken),
                SignUpStep.CodeSent => await HandlePostCodeSentAsync(stepChallenge, model, cancellationToken),
                SignUpStep.FinalMessage => HandleUnallowed(new ResultError($"Something called {nameof(HandlePostVerifyCodeAsync)} in not supported mode, with {stepChallenge} and {model}")),
                _ => await HandlePostSetEmailAsync(stepChallenge, model, cancellationToken), // this is EnterEmail too
            };
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
