using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Constants;
using Bureau.Identity.Models;
using Bureau.Identity.Data;
using Bureau.Identity.Factories;
using Bureau.Identity.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace Bureau.Identity.Managers
{
    internal class InternalUserManager : UserManager<ApplicationUser>
    {
        private readonly IServiceProvider _serviceProvider;
        public InternalUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _serviceProvider = services;
        }

        private bool TryGetBureauUserStore(out BureauUserStore? store) 
        {
            store = Store as BureauUserStore;
            if (store == null)
            {
                Logger.LogCritical(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
                return false;
            }
            return true;
        }

        //TODO [IMPROVE] accept BureauUser and return result
        public virtual async Task<IdentityResult> AddLoginAsync(ApplicationUser user, IBureauLoginModel login)
        {
            ThrowIfDisposed();
            BureauUserStore? store = Store as BureauUserStore;
            if (store == null)
            {
                Logger.LogCritical(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
                return IdentityResultFactory.CriticalError();
            }
            ArgumentNullException.ThrowIfNull(login);
            ArgumentNullException.ThrowIfNull(user);

            var existingUser = await FindByLoginAsync(login.LoginProvider, login.ProviderKey).ConfigureAwait(false);
            if (existingUser != null)
            {
                //TODO Logging
                //Logger.LogWarning(UIMessages.LoginAlreadyAssociatedWithAccount);
                return IdentityResultFactory.LoginAlreadyAssociatedWithAccount();
            }
            await store.AddUserLoginAsync(user, login, CancellationToken.None).ConfigureAwait(false);
            return await UpdateUserAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the <paramref name="login"/> given to the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to add the login to.</param>
        /// <param name="login">The login to add to the user.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task<IdentityResult> AddLoginAsync(ApplicationUser user, ExternalLoginInfo login)
        {
            ThrowIfDisposed();
            BureauUserStore? store = Store as BureauUserStore;
            if (store == null) 
            {
                Logger.LogCritical(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
                return IdentityResultFactory.CriticalError();
            }
            ArgumentNullException.ThrowIfNull(login);
            ArgumentNullException.ThrowIfNull(user);

            var existingUser = await FindByLoginAsync(login.LoginProvider, login.ProviderKey).ConfigureAwait(false);
            if (existingUser != null)
            {
                //TODO Logging
                //Logger.LogWarning(UIMessages.LoginAlreadyAssociatedWithAccount);
                return IdentityResultFactory.LoginAlreadyAssociatedWithAccount();
            }
            string email = login.Principal.HasClaim(c => c.Type == ClaimTypes.Email) ? login.Principal.FindFirstValue(ClaimTypes.Email)! : string.Empty;

            BureauUserLoginModel loginInfo = new BureauUserLoginModel(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName, user.Id, email);
            await store.AddUserLoginAsync(user, loginInfo, CancellationToken.None).ConfigureAwait(false);
            return await UpdateUserAsync(user).ConfigureAwait(false);
        }

        [Obsolete]
        public override Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            return base.AddLoginAsync(user, login);
        }

        /// <summary>
        /// Retrieves the associated logins for the specified <param ref="user"/>.
        /// </summary>
        /// <param name="user">The user whose associated logins to retrieve.</param>
        /// <returns>
        /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="BureauUserLoginModel"/> for the specified <paramref name="user"/>, if any.
        /// </returns>
        public virtual async Task<IList<BureauUserLoginModel>> GetUserLoginsAsync(ApplicationUser user) 
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            if (TryGetBureauUserStore(out BureauUserStore? store))
            {
                return await store!.GetUserLoginsAsync(user, CancellationToken).ConfigureAwait(false);
            }
            else
            {
                Logger.LogCritical(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
                throw new NullReferenceException(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
            }
        }


        [Obsolete($"Use {nameof(GetUserLoginsAsync)} instead")]
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return base.GetLoginsAsync(user);
        }

        /// <summary>
        /// Returns an authentication token for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <returns>The authentication token for a user</returns>
        public virtual Task<string?> GetAuthenticationTokenAsync(ApplicationUser user, string loginProvider, string providerKey, string tokenName)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(loginProvider);
            ArgumentNullException.ThrowIfNull(providerKey);
            ArgumentNullException.ThrowIfNull(tokenName);
            if (TryGetBureauUserStore(out BureauUserStore? store)) 
            {
                return store!.GetTokenAsync(user, loginProvider, providerKey, tokenName, CancellationToken);
            }
            else 
            {
                Logger.LogCritical(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
                throw new NullReferenceException(ErrorMessages.BureauVariableIsNull.Format(nameof(BureauUserStore)));
            }
        }

        [Obsolete]
        public override Task<string?> GetAuthenticationTokenAsync(ApplicationUser user, string loginProvider, string tokenName)
        {
            return base.GetAuthenticationTokenAsync(user, loginProvider, tokenName);
        }

        /// <summary>
        /// Sets an authentication token for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="providerKey">Unique provider identifier for this login.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <param name="tokenValue">The value of the token.</param>
        /// <returns>Whether the user was successfully updated.</returns>
        public virtual async Task<IdentityResult> SetAuthenticationTokenAsync(ApplicationUser user, string loginProvider, string providerKey, string tokenName, string? tokenValue)
        {
            ThrowIfDisposed();
            BureauUserStore? store;
            if (!TryGetBureauUserStore(out store)) 
            {
                return IdentityResultFactory.CriticalError();
            }
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(loginProvider);
            ArgumentNullException.ThrowIfNull(providerKey);
            ArgumentNullException.ThrowIfNull(tokenName);

            // REVIEW: should updating any tokens affect the security stamp?
            await store!.SetTokenAsync(user, loginProvider, providerKey, tokenName, tokenValue, CancellationToken).ConfigureAwait(false);
            return await UpdateUserAsync(user).ConfigureAwait(false);
        }

        [Obsolete]
        public override Task<IdentityResult> SetAuthenticationTokenAsync(ApplicationUser user, string loginProvider, string tokenName, string? tokenValue)
        {
            return base.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);
        }

        // TODO
        public async Task<Result<List<BureauUserLoginModel>>> GetUserLoginsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await GetUserAsync(principal);

            IList<BureauUserLoginModel> logins = await GetUserLoginsAsync(user!);

            return logins.ToList();
        }

        public async Task<Result<Dictionary<string, string?>>> GetUserLoginTokensAsync(GetUserLoginTokensRequest request, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string?> result = new Dictionary<string, string?>();
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ApplicationUser? user = await dbContext.Users.Where(x => x.Id == request.UserId).FirstOrDefaultAsync();


                result.Add(OpenIdConnectParameterNames.AccessToken, 
                    await dbContext.UserTokens
                    .Where(x => x.UserId == request.UserId && 
                        x.LoginProvider == request.LoginProvider &&
                        x.ProviderKey == request.ProviderKey &&
                        x.Name == OpenIdConnectParameterNames.AccessToken)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync());
                result.Add(OpenIdConnectParameterNames.TokenType, 
                    await dbContext.UserTokens
                    .Where(x => x.UserId == request.UserId &&
                        x.LoginProvider == request.LoginProvider &&
                        x.ProviderKey == request.ProviderKey &&
                        x.Name == OpenIdConnectParameterNames.TokenType)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync());
                result.Add(BureauIdentityConstants.ExpiresAtTokenName,
                    await dbContext.UserTokens
                    .Where(x => x.UserId == request.UserId &&
                        x.LoginProvider == request.LoginProvider &&
                        x.ProviderKey == request.ProviderKey &&
                        x.Name == BureauIdentityConstants.ExpiresAtTokenName)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync());
                result.Add(OpenIdConnectParameterNames.RefreshToken,
                    await dbContext.UserTokens
                    .Where(x => x.UserId == request.UserId &&
                        x.LoginProvider == request.LoginProvider &&
                        x.ProviderKey == request.ProviderKey &&
                        x.Name == OpenIdConnectParameterNames.RefreshToken)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync());
            }
            return result;
        }

        public async Task<Result> UpdateUserLoginTokensAsync(UpdateUserLoginTokensRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    Dictionary<string, ApplicationUserToken> dict = await dbContext.UserTokens
                            .Where(x => x.UserId == request.UserLogin.UserId &&
                                x.LoginProvider == request.UserLogin.LoginProvider &&
                                x.ProviderKey == request.UserLogin.ProviderKey)
                            .ToDictionaryAsync(k => k.Name, v => v);


                    foreach (string tokenName in request.Tokens.Keys)
                    {
                        if (dict.ContainsKey(tokenName))
                        {
                            dict[tokenName].Value = request.Tokens[tokenName];
                        }
                        else
                        {
                            dbContext.UserTokens.Add(new ApplicationUserToken()
                            {
                                UserId = request.UserLogin.UserId,
                                LoginProvider = request.UserLogin.LoginProvider,
                                ProviderKey = request.UserLogin.ProviderKey,
                                Name = tokenName,
                                Value = request.Tokens[tokenName]
                            });
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex) 
            {
                return ex;
            }
        }
    }
}
