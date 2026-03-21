using Microsoft.AspNetCore.Mvc;

namespace Sven.PageModels.Connect.SignUp
{
    public interface ISignUpFlowDispatcher
    {
        Task<IActionResult> HandleGetAsync(SignUpStepContext context, CancellationToken cancellationToken = default);
        Task<IActionResult> HandlePostAsync(SignUpStepContext context, CancellationToken cancellationToken = default);
    }
}
