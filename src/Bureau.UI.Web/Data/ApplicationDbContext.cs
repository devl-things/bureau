using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bureau.UI.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : 
        IdentityDbContext<ApplicationUser,IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, ApplicationUserLogin, IdentityRoleClaim<string>, IdentityUserToken<string>>(options)
    {
    }
}
