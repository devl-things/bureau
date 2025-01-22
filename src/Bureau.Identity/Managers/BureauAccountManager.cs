using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Extensions;
using Bureau.Identity.Factories;
using Bureau.Identity.Mappers;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Bureau.Identity.Managers
{
    public class BureauAccountManager : IAccountManager
    {
        private ApplicationUser? _currentUser;

        public IUserManager UserManager { get; set; }
        private readonly InternalUserManager _internalUserManager;

        private string AuthenticatorTokenProvider
        {
            get 
            {
                return _internalUserManager.Options.Tokens.AuthenticatorTokenProvider;
            }
        }
        public bool RequireConfirmedAccount
        {
            get
            {
                return _internalUserManager.Options.SignIn.RequireConfirmedAccount;
            }
        }

        internal BureauAccountManager(InternalUserManager internalUserManager, IUserManager userManager)
        {
            _internalUserManager = internalUserManager;
            UserManager = userManager;
        }

        private async Task<ApplicationUser?> GetApplicationUserAsync(IUserId userId) 
        {
            if (_currentUser == null || _currentUser.Id != userId.Id) 
            {
                _currentUser = await _internalUserManager.FindByIdAsync(userId.Id);
            }
            return _currentUser;
        }

        #region Email
        public async Task<Result> ChangeEmailAsync(IUserId userId, string email, string code)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null) 
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.ChangeEmailAsync(appUser, email, code);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        public async Task<Result> ConfirmEmailAsync(IUserId userId, string code)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.ConfirmEmailAsync(appUser, code);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        /// <summary>
        /// needs IUserManager
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<bool>> IsEmailConfirmedAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            bool internalResult = await _internalUserManager.IsEmailConfirmedAsync(appUser);

            return internalResult;
        }

        public async Task<Result<string>> GenerateChangeEmailTokenAsync(IUserId userId, string newEmail)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<string>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }

            string internalResult = await _internalUserManager.GenerateChangeEmailTokenAsync(appUser, newEmail);

            return new Result<string>(internalResult);
        }

        public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<string>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }

            string internalResult = await _internalUserManager.GenerateEmailConfirmationTokenAsync(appUser);

            return new Result<string>(internalResult);
        }
        #endregion

        #region Password
        public async Task<Result> ChangePasswordAsync(IUserId userId, string currentPassword, string newPassword)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.ChangePasswordAsync(appUser, currentPassword, newPassword);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }
        public async Task<Result> ResetPasswordAsync(string userEmail, string token, string newPassword)
        {
            ApplicationUser? appUser = await _internalUserManager.FindByEmailAsync(userEmail);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.ResetPasswordAsync(appUser, token, newPassword);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }
        public async Task<Result> SetPasswordAsync(IUserId userId, string password)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.AddPasswordAsync(appUser, password);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }
        public async Task<Result<bool>> IsPasswordSetAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            bool internalResult = await _internalUserManager.HasPasswordAsync(appUser);

            return internalResult;
        }
        public async Task<Result<bool>> IsPasswordValidAsync(IUserId userId, string password)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            bool internalResult = await _internalUserManager.CheckPasswordAsync(appUser, password);

            return internalResult;
        }
        public async Task<Result<string>> GeneratePasswordResetTokenAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<string>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }

            string internalResult = await _internalUserManager.GenerateEmailConfirmationTokenAsync(appUser);

            return new Result<string>(internalResult);
        }
        #endregion

        #region Two factor
        public async Task<Result> SetTwoFactorEnabledAsync(IUserId userId, bool enabled)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.SetTwoFactorEnabledAsync(appUser, enabled);

            if (internalResult.Succeeded) 
            {
                return new Result();
            }
            return internalResult.ToResult();
        }
        public async Task<Result<bool>> IsTwoFactorEnabledAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            bool internalResult = await _internalUserManager.GetTwoFactorEnabledAsync(appUser);

            return internalResult;
        }
        public async Task<Result<bool>> IsTwoFactorTokenValidAsync(IUserId userId, string token)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            bool internalResult = await _internalUserManager.VerifyTwoFactorTokenAsync(appUser, AuthenticatorTokenProvider, token);

            return internalResult;
        }
        public async Task<Result<List<string>?>> GenerateNewTwoFactorRecoveryCodesAsync(IUserId userId, int number)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<List<string>?>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }
            IEnumerable<string>? internalResult = await _internalUserManager.GenerateNewTwoFactorRecoveryCodesAsync(appUser, number);

            return internalResult?.ToList();
        }
        public async Task<Result<int>> CountRecoveryCodesAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            int internalResult = await _internalUserManager.CountRecoveryCodesAsync(appUser);

            return internalResult;
        }
        public async Task<Result<string?>> GetAuthenticatorKeyAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<string?>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }

            string? internalResult = await _internalUserManager.GetAuthenticatorKeyAsync(appUser);

            return new Result<string?>(internalResult);
        }
        public async Task<Result> ResetAuthenticatorKeyAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.ResetAuthenticatorKeyAsync(appUser);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }
        #endregion

        #region Phone number
        public async Task<Result> SetPhoneNumberAsync(BureauUser userId, string? phoneNumber)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }

            IdentityResult internalResult = await _internalUserManager.SetPhoneNumberAsync(appUser, phoneNumber);

            if (internalResult.Succeeded)
            {
                return new Result();
            }
            return internalResult.ToResult();
        }

        public async Task<Result<string?>> GetPhoneNumberAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);

            if (appUser == null)
            {
                return new Result<string?>(new ResultError(ErrorMessages.BureauVariableIsNull.Format(nameof(appUser))));
            }

            string? internalResult = await _internalUserManager.GetPhoneNumberAsync(appUser);

            return new Result<string?>(internalResult);
        }
        #endregion

        #region External login
        public async Task<Result> UpdateExternalAuthenticationTokensAsync(BureauExternalLogin externalLogin)
        {
            if (externalLogin == null) 
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(externalLogin));
            }

            if (externalLogin.AuthenticationTokens != null && externalLogin.AuthenticationTokens.Any())
            {
                ApplicationUser? appUser = await _internalUserManager.FindByLoginAsync(externalLogin.LoginProvider, externalLogin.ProviderKey);
                if (appUser == null)
                {
                    return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
                }
                //TODO [IMPROVE] insert in bulk
                foreach (var token in externalLogin.AuthenticationTokens)
                {
                    IdentityResult result = await _internalUserManager!.SetAuthenticationTokenAsync(appUser, externalLogin.LoginProvider, externalLogin.ProviderKey, token.Name, token.Value);
                    if (!result.Succeeded)
                    {
                        return result.ToResult();
                    }
                }
            }

            return new Result();
        }
        #endregion
    }
}
