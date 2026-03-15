using Microsoft.AspNetCore.Mvc;
using Sven.PageModels.Components;
using System.Security.Claims;

namespace Sven.Pages.Shared.Components.UserNav
{
    public class UserNavViewComponent : ViewComponent
    {
        public UserNavViewComponent()
        {

        }
        public IViewComponentResult Invoke()
        {
            ClaimsPrincipal user = HttpContext.User;

            UserNavViewModel model = new()
            {
                Email = user.Identity?.Name,
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false
            };

            return View(model);
        }
    }
}
