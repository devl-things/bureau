using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Extensions;
using Bureau.Identity.Mappers;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Bureau.Identity.Managers
{
    public class BureauUserManager : IUserManager
    {
        private BureauUser? _currentUser;
        private ApplicationUser? _currentAppUser;

        private readonly InternalUserManager _internalUserManager;

        internal BureauUserManager(InternalUserManager internalUserManager)
        {
            _internalUserManager = internalUserManager;
        }
        private async Task<ApplicationUser?> GetApplicationUserAsync(IUserId userId)
        {
            if (_currentAppUser == null || _currentAppUser.Id != userId.Id)
            {
                _currentAppUser = await _internalUserManager.FindByIdAsync(userId.Id);
            }
            return _currentAppUser;
        }

        #region Done

        public async Task<Result<IUserId>> GetUserIdByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            if (_currentUser == null || _currentUser.UserName != userName) 
            {
                ApplicationUser? appUser = await _internalUserManager.FindByNameAsync(userName);
                if (appUser == null)
                {
                    return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
                }
                _currentUser = appUser.ToBureauUser();
            }
            return new Result<IUserId>(_currentUser);
        }

        public async Task<Result<IUserId>> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (_currentUser == null || _currentUser.Email != email)
            {
                ApplicationUser? appUser = await _internalUserManager.FindByEmailAsync(email);
                if (appUser == null)
                {
                    return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
                }
                _currentUser = appUser.ToBureauUser();
            }
            return new Result<IUserId>(_currentUser);
        }
        public async Task<Result<BureauUser>> AddUserAsync(string userName, string? email, CancellationToken cancellationToken = default)
        {
            ApplicationUser dbUser = PrepareApplicationUser(userName, email);
            IdentityResult result = await _internalUserManager.CreateAsync(dbUser);
            if (result.Succeeded)
            {
                return dbUser.ToBureauUser();
            }
            return result.ToResultError();
        }
        public async Task<Result<BureauUser>> AddUserAsync(string userName, string? email, string password, CancellationToken cancellationToken = default)
        {
            ApplicationUser dbUser = PrepareApplicationUser(userName, email);
            IdentityResult result = await _internalUserManager.CreateAsync(dbUser, password);
            if (result.Succeeded)
            {
                return dbUser.ToBureauUser();
            }
            return result.ToResultError();
        }

        private ApplicationUser PrepareApplicationUser(string userName, string? email) 
        {
            return new ApplicationUser() { UserName = userName, Email = email };
        }
        public async Task<Result> RemoveUserAsync(IUserId userId, CancellationToken cancellationToken = default)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            IdentityResult internalResult = await _internalUserManager.DeleteAsync(appUser);
            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        //TODO [IMPROVE] this could be better without get user first
        public async Task<Result<List<BureauUserLoginModel>>> GetUserLoginsAsync(IUserId userId, CancellationToken cancellationToken = default) 
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            IList<BureauUserLoginModel> internalResult = await _internalUserManager.GetUserLoginsAsync(appUser);
            return internalResult.ToList();
        }

        //TODO [IMPROVE] this could be better without get user first
        public async Task<Result> RemoveUserLoginAsync(IUserId userId, IBureauLoginModel login, CancellationToken cancellationToken = default)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            IdentityResult internalResult = await _internalUserManager.RemoveLoginAsync(appUser, login.LoginProvider, login.ProviderKey);
            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        public Task<Result> UpdateUserLoginTokensAsync(UpdateUserLoginTokensRequest request, CancellationToken cancellationToken = default)
        {
            return _internalUserManager.UpdateUserLoginTokensAsync(request, cancellationToken);
        }

        public Task<Result<Dictionary<string, string?>>> GetUserLoginTokensAsync(GetUserLoginTokensRequest request, CancellationToken cancellationToken = default)
        {
            return _internalUserManager.GetUserLoginTokensAsync(request, cancellationToken);
        }

        public async Task<Result> AddUserLoginAsync(IUserId userId, IBureauLoginModel login, CancellationToken cancellationToken = default)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            IdentityResult internalResult = await _internalUserManager.AddLoginAsync(appUser, login);
            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        public async Task<Result<BureauUser>> GetUserByIdAsync(IUserId userId, CancellationToken cancellationToken = default)
        {
            ApplicationUser? appUser = await _internalUserManager.FindByIdAsync(userId.Id);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            return appUser.ToBureauUser();
        }
        #endregion

        //public Task<Result> AddUserLoginAsync(BureauUser user, IBureauLoginModel login, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}



        //public Task<Result<List<BureauUserLoginModel>>> GetUserLoginsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<Result> AddUserAsync(BureauUser user, CancellationToken cancellationToken = default)
        //{
        //    //TODO set id
        //    throw new NotImplementedException();
        //}



        //public Task<Result<BureauUser>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<Result<BureauUser>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        //{
        //    //if null
        //    //"\"Unable to find user with Id '{userId}'\""
        //    throw new NotImplementedException();
        //}

        //public Task<Result<BureauUser>> GetUserByNameAsync(string userName, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}


        //public Task<Result> SetUserEmailAsync(BureauUser user, string? email, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<Result> SetUserUserNameAsync(BureauUser user, string userName, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
