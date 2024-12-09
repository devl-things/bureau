using Bureau.Identity.Models;
using Bureau.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bureau.Identity.Repositories
{
    internal class BureauUserStore : UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, ApplicationUserLogin, ApplicationUserToken, IdentityRoleClaim<string>>
    {
        public BureauUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
        {
        }

        /// <summary>
        /// Adds the <paramref name="login"/> given to the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to add the login to.</param>
        /// <param name="login">The login to add to the user.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual Task AddUserLoginAsync(ApplicationUser user, IBureauLoginModel login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(login);
            Context.Set<ApplicationUserLogin>().Add(CreateApplicationUserLogin(user, login));
            return Task.FromResult(false);
        }

        [Obsolete($"Use {nameof(AddUserLoginAsync)} instead")]
        public override Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            return base.AddLoginAsync(user, login, cancellationToken);
        }

        protected virtual ApplicationUserLogin CreateApplicationUserLogin(ApplicationUser user, IBureauLoginModel login)
        {
            return new ApplicationUserLogin
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                Email = login.Email
            };
        }

        /// <summary>
        /// Retrieves the associated logins for the specified <param ref="user"/>.
        /// </summary>
        /// <param name="user">The user whose associated logins to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="BureauUserLoginModel"/> for the specified <paramref name="user"/>, if any.
        /// </returns>
        public virtual async Task<IList<BureauUserLoginModel>> GetUserLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            var userId = user.Id;
            return await Context.Set<ApplicationUserLogin>()
                .Where(x => x.UserId.Equals(userId))
                .Select(x => new BureauUserLoginModel(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName, x.UserId, x.Email))
                .ToListAsync(cancellationToken);
        }

        [Obsolete($"Use {nameof(GetUserLoginsAsync)} instead")]
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return base.GetLoginsAsync(user, cancellationToken);
        }


        /// <summary>
        /// Sets the token value for a particular user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="loginProvider">The authentication provider for the token.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="value">The value of the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task SetTokenAsync(ApplicationUser user, string loginProvider, string providerKey, string name, string? value, CancellationToken cancellationToken)
        {

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(user);

            var token = await FindTokenAsync(user, loginProvider, providerKey, name, cancellationToken).ConfigureAwait(false);
            if (token == null)
            {
                await AddUserTokenAsync(CreateApplicationUserToken(user, loginProvider, providerKey, name, value)).ConfigureAwait(false);
            }
            else
            {
                token.Value = value;
            }
        }

        [Obsolete]
        public override Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
        {
            return base.SetTokenAsync(user, loginProvider, name, value, cancellationToken);
        }

        /// <summary>
        /// Called to create a new instance of a <see cref="IdentityUserToken{TKey}"/>.
        /// </summary>
        /// <param name="user">The associated user.</param>
        /// <param name="loginProvider">The associated login provider.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="name">The name of the user token.</param>
        /// <param name="value">The value of the user token.</param>
        /// <returns></returns>
        protected virtual ApplicationUserToken CreateApplicationUserToken(ApplicationUser user, string loginProvider, string providerKey, string name, string? value)
        {
            return new ApplicationUserToken
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                ProviderKey = providerKey,
                Name = name,
                Value = value
            };
        }

        /// <summary>
        /// Find a user token if it exists.
        /// </summary>
        /// <param name="user">The token owner.</param>
        /// <param name="loginProvider">The login provider for the token.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The user token if it exists.</returns>
        protected virtual Task<ApplicationUserToken?> FindTokenAsync(ApplicationUser user, string loginProvider, string providerKey, string name, CancellationToken cancellationToken)
        {
            return Context.Set<ApplicationUserToken>().FindAsync(new object[] { user.Id, loginProvider, name, providerKey }, cancellationToken).AsTask();
        }

        [Obsolete]
        protected override Task<ApplicationUserToken?> FindTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return FindTokenAsync(user, loginProvider, string.Empty, name, cancellationToken);
        }

        /// <summary>
        /// Returns the token value.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="loginProvider">The authentication provider for the token.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string providerKey, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(user);
            var entry = await FindTokenAsync(user, loginProvider, providerKey, name, cancellationToken).ConfigureAwait(false);
            return entry?.Value;
        }

        [Obsolete]
        public override Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return base.GetTokenAsync(user, loginProvider, name, cancellationToken);
        }
    }
}
