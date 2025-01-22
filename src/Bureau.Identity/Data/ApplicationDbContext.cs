using Bureau.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Identity.Data
{
    internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : 
        IdentityDbContext<ApplicationUser,IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, ApplicationUserLogin, IdentityRoleClaim<string>, ApplicationUserToken>(options)
    {

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUserToken>()
                .HasKey(c => new { c.UserId, c.LoginProvider, c.Name, c.ProviderKey });
        }
    }
}
