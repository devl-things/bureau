using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Models;
using Sven.Services;

namespace Sven.PageModels
{
    [Authorize]
    public class AuthPageModel : PageModel
    {
        protected readonly ICurrentUserProvider _currentUserProvider;

        public SvenUser CurrentUser => _currentUserProvider.CurrentUser;
        public AuthPageModel(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }
    }
}
