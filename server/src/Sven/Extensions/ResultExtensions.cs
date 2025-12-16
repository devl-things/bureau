using Bureau;
using Microsoft.AspNetCore.Mvc;
using Sven.Models;

namespace Sven.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToOAuthError(this ResultError resultError)
        {
            string error = resultError.ErrorMessage;
            //TODO #96
            string? userMessage = resultError.ErrorMessage;

            OAuthError payload = new OAuthError(error, userMessage);
            return new BadRequestObjectResult(payload);
        }
    }
}
