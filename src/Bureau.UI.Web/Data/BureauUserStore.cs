using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bureau.UI.Web.Data
{
    public class BureauUserStore : UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, ApplicationUserLogin, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        ExternalLoginInfo _login;

        public BureauUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
        {
        }

        public virtual Task AddLoginAsync(ApplicationUser user, ExternalLoginInfo login, CancellationToken cancellationToken = default)
        {
            _login = login;
            return base.AddLoginAsync(user, login, cancellationToken);
        }
        protected override ApplicationUserLogin CreateUserLogin(ApplicationUser user, UserLoginInfo login)
        {
            ApplicationUserLogin l = base.CreateUserLogin(user, login);
            l.Email = _login.Principal.HasClaim(c => c.Type == ClaimTypes.Email) ? _login.Principal.FindFirstValue(ClaimTypes.Email) : string.Empty;
            return l;
        }
    }
}
