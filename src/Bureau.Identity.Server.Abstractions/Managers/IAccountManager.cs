using Bureau.Core;
using Bureau.Identity.Models;

namespace Bureau.Identity.Managers
{
    public interface IAccountManager
    {
        public IUserManager UserManager
        {
            get;
        }
        public bool RequireConfirmedAccount { get; }

        #region Email
        public Task<Result> ChangeEmailAsync(IUserId userId, string email, string code);
        public Task<Result> ConfirmEmailAsync(IUserId userId, string code);
        public Task<Result<bool>> IsEmailConfirmedAsync(IUserId userId);
        public Task<Result<string>> GenerateChangeEmailTokenAsync(IUserId user, string newEmail);
        public Task<Result<string>> GenerateEmailConfirmationTokenAsync(IUserId user);
        #endregion

        #region Password
        public Task<Result> ChangePasswordAsync(IUserId user, string currentPassword, string newPassword);
        public Task<Result> ResetPasswordAsync(string userEmail, string token, string newPassword);
        public Task<Result> SetPasswordAsync(IUserId user, string password);
        public Task<Result<bool>> IsPasswordSetAsync(IUserId user);
        public Task<Result<bool>> IsPasswordValidAsync(IUserId user, string password);
        public Task<Result<string>> GeneratePasswordResetTokenAsync(IUserId userId);
        #endregion

        #region Two factor
        public Task<Result> SetTwoFactorEnabledAsync(IUserId userId, bool enabled);
        public Task<Result<bool>> IsTwoFactorEnabledAsync(IUserId userId);
        public Task<Result<bool>> IsTwoFactorTokenValidAsync(IUserId userId, string token);
        public Task<Result<List<string>?>> GenerateNewTwoFactorRecoveryCodesAsync(IUserId userId, int number);
        public Task<Result<int>> CountRecoveryCodesAsync(IUserId user);
        public Task<Result<string?>> GetAuthenticatorKeyAsync(IUserId user);
        public Task<Result> ResetAuthenticatorKeyAsync(IUserId user);
        #endregion

        #region Phone number
        public Task<Result<string?>> GetPhoneNumberAsync(IUserId user);
        public Task<Result> SetPhoneNumberAsync(BureauUser user, string? phoneNumber);
        #endregion

        #region External login
        public Task<Result> UpdateExternalAuthenticationTokensAsync(BureauExternalLogin externalLogin);
        #endregion
    }
}
