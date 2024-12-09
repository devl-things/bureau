using Bureau.Identity.Managers;
using Bureau.Identity.Data;
using Bureau.Identity.Models;
using Bureau.Identity.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Bureau.Identity.Providers;

namespace Bureau.Identity.Configurations
{
    public static class BureauIdentityServiceConfiguration
    {
        public static void AddBureauIdentity(this IServiceCollection services, string connectionString) 
        {
            services.AddSingleton<IEmailSender<ApplicationUser>, InternalEmailManager>();
            services.AddSingleton<IEmailManager, BureauEmailManager>();

            services.AddScoped<IIdentityProvider, BureauIdentityProvider>();

            services.AddScoped<IUserManager>(provider =>
            {
                InternalUserManager internalUserManager = provider.GetRequiredService<InternalUserManager>();

                IUserManager service = (IUserManager)Activator.CreateInstance(
                    typeof(BureauUserManager),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { internalUserManager },
                    null)!;

                return service;
            });
            services.AddScoped<ISignInManager>(provider =>
            {
                InternalUserManager internalUserManager = provider.GetRequiredService<InternalUserManager>();
                SignInManager<ApplicationUser> internalSignInManager = provider.GetRequiredService<SignInManager<ApplicationUser>>();

                ISignInManager service = (ISignInManager)Activator.CreateInstance(
                    typeof(BureauSignInManager),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { internalSignInManager, internalUserManager },
                    null)!;

                return service;
                //return (ISignInManager)Activator.CreateInstance(typeof(BureauSignInManager), nonPublic: true)!;
            });
            services.AddScoped<IAccountManager>(provider =>
            {
                InternalUserManager internalUserManager = provider.GetRequiredService<InternalUserManager>();
                IUserManager userManager = provider.GetRequiredService<IUserManager>();

                IAccountManager service = (IAccountManager)Activator.CreateInstance(
                    typeof(BureauAccountManager),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { internalUserManager, userManager },
                    null)!;

                return service;
                //return (IAccountManager)Activator.CreateInstance(typeof(BureauAccountManager), nonPublic: true)!;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
            })
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddUserStore<BureauUserStore>()
               .AddUserManager<InternalUserManager>()
               //.AddSignInManager<InternalSignInManager>()
               .AddSignInManager()
               .AddDefaultTokenProviders()
               .AddApiEndpoints();

            
        }
    }
}
