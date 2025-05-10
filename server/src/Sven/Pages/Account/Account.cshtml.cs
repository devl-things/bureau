using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sven.Pages.Account
{
    [Authorize]
    public class AccountModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
