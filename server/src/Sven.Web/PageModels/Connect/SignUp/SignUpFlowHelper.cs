using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Services;

namespace Sven.PageModels.Connect.SignUp
{
    public class SignUpFlowHelper
    {
        private readonly ILogger<SignUpFlowHelper> _logger;
        private readonly IUserService _userService;

        public SignUpFlowHelper(ILogger<SignUpFlowHelper> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<Result<UserVerificationCode>> VerifyChallengeStatusAsync(
            string challenge, VerificationStatus status, CancellationToken cancellationToken)
        {
            Result<UserVerificationCode> codeResult = await _userService.GetVerificationCodeAsync(challenge, cancellationToken);
            if (codeResult.IsError)
            {
                return ResultError.From(codeResult.Error, string.Format(LogMessages.InvalidChallengeForStep, nameof(VerifyChallengeStatusAsync), challenge));
            }
            if (!codeResult.Value.Status.HasFlag(status))
            {
                return ResultError.From($"Verification status is not good ({codeResult.Value.Status})");
            }
            return codeResult.Value;
        }

        public static string BuildStepUrl(string basePath, StepChallengeRequest stepChallenge)
        {
            if (string.IsNullOrEmpty(stepChallenge.Challenge))
            {
                return $"{basePath}?{ViewConstants.PropertyNames.Step}={stepChallenge.StepShort}";
            }
            return $"{basePath}?{ViewConstants.PropertyNames.Step}={stepChallenge.StepShort}&{AuthConstants.PropertyNames.Challenge}={stepChallenge.Challenge}";
        }

        public IActionResult GoToUrlWithError(SignUpStepContext context, string url, ResultError error)
        {
            _logger.LogResultError(error);
            return context.GoToUrl(url);
        }
    }
}
