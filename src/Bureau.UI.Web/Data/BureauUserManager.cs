using Bureau.UI.Web.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bureau.UI.Web.Data
{
    public class BureauUserManager : UserManager<ApplicationUser>
    {
        public BureauUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public virtual async Task<IdentityResult> AddLoginAsync(ApplicationUser user, ExternalLoginInfo login)
        {
            ThrowIfDisposed();
            BureauUserStore? store = Store as BureauUserStore;
            if (store == null) 
            {
                Logger.LogCritical(LogMessages.IsNull, nameof(BureauUserStore));
                return IdentityResultHelper.CriticalError();
            }
            ArgumentNullException.ThrowIfNull(login);
            ArgumentNullException.ThrowIfNull(user);

            var existingUser = await FindByLoginAsync(login.LoginProvider, login.ProviderKey).ConfigureAwait(false);
            if (existingUser != null)
            {
                Logger.LogWarning(UIMessages.LoginAlreadyAssociatedWithAccount);
                return IdentityResultHelper.LoginAlreadyAssociatedWithAccount();
            }
            await store.AddLoginAsync(user, login, CancellationToken.None).ConfigureAwait(false);
            return await UpdateUserAsync(user).ConfigureAwait(false);
        }
    }
}
