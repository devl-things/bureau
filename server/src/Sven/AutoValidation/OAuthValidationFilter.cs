using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sven.Configurations;
using Sven.Models;

namespace Sven.AutoValidation
{
    public class OAuthValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                string errorDescription = string.Join("; ",
                    context.ModelState.Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(error => $"The '{e.Key}' field is invalid: {error.ErrorMessage}")));
                context.Result = new BadRequestObjectResult(new OAuthError(AuthConstants.OAuth.Errors.InvalidRequest,
                    errorDescription));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
