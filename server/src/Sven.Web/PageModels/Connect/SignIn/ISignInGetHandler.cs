using Microsoft.AspNetCore.Mvc;

namespace Sven.PageModels.Connect.SignIn
{
    public interface ISignInGetHandler
    {
        Task<IActionResult> HandleGetAsync(SignInContext context, CancellationToken cancellationToken = default);
    }
}
