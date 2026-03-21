using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Extensions;

namespace Sven.PageModels.Connect.SignUp
{
    public class ForgotFinalMessageStepHandler : ISignUpStepHandler
    {
        private readonly ILogger<ForgotFinalMessageStepHandler> _logger;

        public SignUpStep Step { get { return SignUpStep.FinalMessage; } }

        public ForgotFinalMessageStepHandler(ILogger<ForgotFinalMessageStepHandler> logger)
        {
            _logger = logger;
        }

        public Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IActionResult>(context.Page());
        }

        public Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(ResultError.From($"POST not allowed for {nameof(ForgotFinalMessageStepHandler)}"));
            return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status405MethodNotAllowed));
        }
    }
}
