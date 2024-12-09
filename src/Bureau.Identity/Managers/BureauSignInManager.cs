using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Mappers;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.Identity.Managers
{
    public class BureauSignInManager : ISignInManager
    {
        private ApplicationUser? _currentAppUser;
        private readonly SignInManager<ApplicationUser> _internalSignInManager;
        private readonly InternalUserManager _internalUserManager;

        internal BureauSignInManager(SignInManager<ApplicationUser> internalSignInManager, InternalUserManager internalUserManager)
        {
            _internalSignInManager = internalSignInManager;
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
        public async Task<Result> RefreshSignInAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            await _internalSignInManager.RefreshSignInAsync(appUser!);
            return new Result();
        }
        public Task<Result> SignInAsync(IUserId user, bool isPersistent, string? authenticationMethod = null)
        {
            return SignInAsync(user, new AuthenticationProperties { IsPersistent = isPersistent }, authenticationMethod);
        }
        public async Task<Result> SignInAsync(IUserId userId, AuthenticationProperties authenticationProperties, string? authenticationMethod = null)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            await _internalSignInManager.SignInAsync(appUser!, authenticationProperties, authenticationMethod);
            return new Result();
        }
        public async Task<Result> SignOutAsync()
        {
            await _internalSignInManager.SignOutAsync();
            return new Result();
        }

        public async Task<Result<BureauExternalLogin>> GetExternalLoginAsync(string? expectedXsrf = null)
        {
            ExternalLoginInfo? internalResult = await _internalSignInManager.GetExternalLoginInfoAsync(expectedXsrf);

            if (internalResult == null) 
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(internalResult));
            }
            return internalResult.ToBureauExternalLogin();

        }
        public async Task<BureauSignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            SignInResult internalResult = await _internalSignInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
            return internalResult.ToBureauSignInResult();
        }
        public Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            return _internalSignInManager.GetExternalAuthenticationSchemesAsync();
        }
        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string? provider, [StringSyntax(StringSyntaxAttribute.Uri)] string? redirectUrl, string? userId = null)
        {
            return _internalSignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
        }

        public async Task<BureauSignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            SignInResult internalResult = await _internalSignInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
            return internalResult.ToBureauSignInResult();
        }
        public async Task<BureauSignInResult> CheckPasswordSignInAsync(IUserId userId, string password, bool lockoutOnFailure)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return new BureauSignInResult(false);
            }
            SignInResult internalResult = await _internalSignInManager.CheckPasswordSignInAsync(appUser!, password, lockoutOnFailure);
            return internalResult.ToBureauSignInResult();
        }

        public async Task<Result<BureauUser>> GetTwoFactorAuthenticationUserAsync()
        {
            ApplicationUser? appUser = await _internalSignInManager.GetTwoFactorAuthenticationUserAsync();
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            return appUser.ToBureauUser();
        }
        public async Task<BureauSignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
        {
            SignInResult internalResult = await _internalSignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            return internalResult.ToBureauSignInResult();
        }
        public async Task<BureauSignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient)
        {
            SignInResult internalResult = await _internalSignInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
            return internalResult.ToBureauSignInResult();
        }
        //TODO [IMPROVE] no need to get appUser
        public async Task<Result<bool>> IsTwoFactorClientRememberedAsync(IUserId userId)
        {
            ApplicationUser? appUser = await GetApplicationUserAsync(userId);
            if (appUser == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(appUser));
            }
            bool internalResult = await _internalSignInManager.IsTwoFactorClientRememberedAsync(appUser!);
            return new Result<bool>(internalResult);
        }
        public Task ForgetTwoFactorClientAsync()
        {
            return _internalSignInManager.ForgetTwoFactorClientAsync();
        }
        #endregion
    }
}
