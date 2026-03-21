using Microsoft.AspNetCore.Mvc;

namespace Sven.PageModels.Connect.SignIn
{
    public interface ISignInPostHandler
    {
        Task<IActionResult> HandlePostAsync(SignInContext context, CancellationToken cancellationToken = default);
    }
}
