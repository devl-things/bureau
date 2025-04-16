using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven.Extensions;
using Sven.Models;
using Sven.Services;

namespace Sven.Pages.Connect
{
    public class AuthorizeModel : PageModel
    {
        private readonly IStore<string, OAuthRequest> _pkceRequestStore;

        public AuthorizeModel(IStore<string, OAuthRequest> pkceRequestStore)
        {
            _pkceRequestStore = pkceRequestStore;
        }

        public IActionResult OnGet()
        {
            if (!Request.TryGetCookieValue(AuthConstants.CookieNames.PkceKey, out string? pkceKey))
            {
                return BadRequest("Missing authorization state.");
            }

            if (!_pkceRequestStore.Exists(pkceKey))
            {
                return BadRequest("Invalid or expired request.");
            }

            return Page(); // renders the login page
        }
    }
}
