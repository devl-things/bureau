using Bureau.Core;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.Identity.Managers
{
    public interface ISignInManager
    {
        public Task<Result> RefreshSignInAsync(IUserId value);
        public Task<Result> SignInAsync(IUserId user, bool isPersistent, string? authenticationMethod = null);
        public Task<Result> SignInAsync(IUserId user, AuthenticationProperties authenticationProperties, string? authenticationMethod = null);
        public Task<Result> SignOutAsync();

        public Task<Result<BureauExternalLogin>> GetExternalLoginAsync(string? expectedXsrf = null);
        public Task<BureauSignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor);
       
        public Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string? provider, [StringSyntax(StringSyntaxAttribute.Uri)] string? redirectUrl, string? userId = null);
        
        public Task<BureauSignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);
        public Task<BureauSignInResult> CheckPasswordSignInAsync(IUserId user, string password, bool lockoutOnFailure);

        public Task<Result<BureauUser>> GetTwoFactorAuthenticationUserAsync();
        public Task<BureauSignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);
        public Task<BureauSignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient);
        public Task<Result<bool>> IsTwoFactorClientRememberedAsync(IUserId user);
        public Task ForgetTwoFactorClientAsync();        
    }
}
