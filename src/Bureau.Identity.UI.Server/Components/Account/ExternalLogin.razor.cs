using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Identity.Constants;
using Bureau.Identity.Factories;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Bureau.Identity.UI.Constants;
using Bureau.Identity.UI.Models;
using Bureau.UI.Components;
using Bureau.UI.Managers;
using Bureau.UI.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ExternalLogin
    {
        //private ExternalLoginInfo? _externalLoginInfo;
        private BureauExternalLogin _externalLogin = default!;
        private string? _providerDisplayName => _externalLogin?.ProviderDisplayName;

        private StatusMessageInput _statusMessage = new StatusMessageInput();

        [SupplyParameterFromQuery(Name = BureauIdentityQueryParameterNames.ReturnUrl)]
        private string? _returnUrl { get; set; }

        [SupplyParameterFromQuery(Name = BureauIdentityQueryParameterNames.RemoteError)]
        private string? _remoteError { get; set; }

        [SupplyParameterFromQuery(Name = BureauIdentityQueryParameterNames.Action)]
        private string? _action { get; set; }

        [CascadingParameter]
        private HttpContext _httpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel _input { get; set; } = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Inject]
        private ISignInManager _signInManager { get; init; }
        [Inject]
        private IUserManager _userManager { get; init; }
        [Inject]
        private IAccountManager _accountManager { get; init; }
        //[Inject]
        //private IUserStore<ApplicationUser> _userStore { get; init; }

        [Inject]
        IEmailManager _emailManager { get; init; }
        [Inject]
        private BureauRedirectManager _redirectManager { get; init; }
        [Inject]
        private ILogger<ExternalLogin> _logger { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        protected override async Task OnInitializedAsync()
        {   
            if (_remoteError is not null)
            {
                _logger.LogError(BureauIdentityMessages.ErrorFromExternalProvider, _remoteError);
                _redirectManager.RedirectToWithStatus(BureauIdentityUIUris.Login, BureauIdentityMessages.ErrorFromExternalProvider.Format(_remoteError), _httpContext);
            }

            Result<BureauExternalLogin> externalLoginResult = await _signInManager.GetExternalLoginAsync();
            if (externalLoginResult.IsError) 
            {
                _logger.LogError(BureauIdentityMessages.ErrorLoadingExternalLogin);
                _redirectManager.RedirectToWithStatus(BureauIdentityUIUris.Login, BureauIdentityMessages.ErrorLoadingExternalLogin, _httpContext);
            }
            _externalLogin = externalLoginResult.Value;
            if (HttpMethods.IsGet(_httpContext.Request.Method))
            {
                if (_action == BureauIdentityCallbackActionNames.Login)
                {
                    await OnLoginCallbackAsync();
                    return;
                }

                // We should only reach this page via the login callback, so redirect back to
                // the login page if we get here some other way.
                _redirectManager.RedirectTo(BureauIdentityUIUris.Login);
            }
        }

        private async Task OnLoginCallbackAsync()
        {
            // Sign in the user with this external login provider if the user already has a login.
            BureauSignInResult result = await _signInManager.ExternalLoginSignInAsync(
                loginProvider: _externalLogin?.LoginProvider!,
                providerKey: _externalLogin?.ProviderKey!,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.IsSuccess)
            {
                Result savedTokensResult = await _accountManager.UpdateExternalAuthenticationTokensAsync(_externalLogin!);

                //TODO
                if (savedTokensResult.IsError) 
                {
                    _logger.Warning($"Error: {savedTokensResult.Error.ErrorMessage}");
                }
                _logger.Info(BureauIdentityMessages.UserLoginInWithProvider,
                    _externalLogin!.Principal.Identity?.Name,
                    _externalLogin!.LoginProvider);
                _redirectManager.RedirectTo(_returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _redirectManager.RedirectTo(BureauIdentityUIUris.Lockout);
            }

            // If the user does not have an account, then ask the user to create an account.
            _input.Email = _externalLogin!.Email;
        }

        private async Task OnValidSubmitAsync()
        {
            if (_input.HasErrors(out string errorMessage)) 
            {
                _statusMessage.SetError(errorMessage);
                return;
            }
            IUserId userId;
            if (_input.HasPassword)
            {
                Result<IUserId> userIdResult = await _accountManager.UserManager.GetUserIdByNameAsync(_input.UserNameOrEmail);
                //Result<BureauUser> userResult = await _userManager.GetUserByNameAsync(_input.UserNameOrEmail);
                if (userIdResult.IsError)
                {
                    //TODO [StatusMessage]
                    _statusMessage.SetError(BureauIdentityUIMessages.AccountDoesntExist);
                    return;
                }
                userId = userIdResult.Value;
                //user = userResult.Value;

                Result<bool> isPasswordSetResult = await _accountManager.IsPasswordSetAsync(userId);
                if (isPasswordSetResult.IsSuccess && !isPasswordSetResult.Value) 
                {
                    _statusMessage.SetWarning(BureauIdentityUIMessages.ExistingUserByWrongPassword);
                    return;
                }
                



                if (!string.IsNullOrWhiteSpace(_externalLogin.Email)) 
                {
                    Result<IUserId> userByEmailResult = await _userManager.GetUserIdByEmailAsync(_externalLogin.Email);

                    if (userByEmailResult.IsSuccess && userId.Id != userByEmailResult.Value.Id)
                    {
                        _logger.LogError(BureauIdentityMessages.UserTriedDuplicatingAccount, _externalLogin!.LoginProvider, _externalLogin.ProviderKey, userId.Id, userByEmailResult.Value.Id);
                        _statusMessage.SetError(BureauIdentityUIMessages.ExistingDoubleUser);
                        return;
                    }
                }

                BureauSignInResult signInResult = await _signInManager.CheckPasswordSignInAsync(userId, _input.Password!, lockoutOnFailure: false);
                if (signInResult.RequiresTwoFactor)
                {
                    // TODO #6
                    _redirectManager.RedirectTo(new LoginWith2faPageAddress(_returnUrl!, false));
                }
                else if (signInResult.IsLockedOut)
                {
                    // TODO #5
                    _logger.Warning(BureauIdentityMessages.UserLockedOut, _input.UserNameOrEmail);
                    _redirectManager.RedirectTo(BureauIdentityUIUris.Lockout);
                }
                else if (!signInResult.IsSuccess)
                {
                    _statusMessage.SetError(BureauIdentityUIMessages.InvalidLogin);
                    return;
                }
            }
            else
            {
                Result<BureauUser> userResult = await _userManager.AddUserAsync(_input.UserNameOrEmail, _input.Email);

                if (userResult.IsError)
                {
                    SetMessage(userResult.Error);
                    return;
                }
                userId = userResult.Value;
            }

            Result result = await _userManager.AddUserLoginAsync(userId, _externalLogin);
            if (result.IsError) 
            {
                SetMessage(result.Error);
                return;
            }
            _logger.Info(BureauIdentityMessages.UserCreatedWithProvider, userId.Id, _externalLogin.LoginProvider);


            Result<string> codeResult = await _accountManager.GenerateEmailConfirmationTokenAsync(userId);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));

            string callbackUrl = _redirectManager.BureauNavigationManager.GetUriForBasePageAddress(new ConfirmEmailPageAddress(userId.Id, code));

            await _emailManager.SendConfirmationLinkAsync(userId, _input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (_accountManager.RequireConfirmedAccount)
            {
                _redirectManager.RedirectTo(new RegisterConfirmationPageAddress(_input.UserNameOrEmail));
            }

            if (_externalLogin.AuthenticationTokens != null && _externalLogin.AuthenticationTokens.Any())
            {
                AuthenticationProperties props = new AuthenticationProperties();
                props.StoreTokens(_externalLogin.AuthenticationTokens);
                props.IsPersistent = true;
                //TODO [error handling]
                await _signInManager.SignInAsync(userId, props, _externalLogin.LoginProvider);
            }
            else 
            {
                //TODO [error handling]
                await _signInManager.SignInAsync(userId, isPersistent: false, _externalLogin.LoginProvider);
            }
            

            
            _redirectManager.RedirectTo(_returnUrl);
        }

        //TODO #12-2 [StatusMessage]
        private void SetMessage(ResultError result)
        {
            //TODO
            _logger.Warning($"Error: {result.ErrorMessage }");
            _statusMessage.SetError($"Error: {result.ErrorMessage}");
        }
        //private void SetMessage(IdentityResult result)
        //{
        //    //TODO
        //    _logger.Warning($"Error: {string.Join(",", result.Errors.Select(error => error.Description))}");
        //    _statusMessage.SetError($"Error: {string.Join(",", result.Errors.Select(error => error.Description))}");
        //}


        private sealed class InputModel
        {
            [EmailAddressFormat]
            [Display(Name = "Email")]
            public string? Email { get; set; } = null;

            [MaxLength(256, ErrorMessage = "The {0} must be at max {1} characters long.")]
            [Display(Name = "User name", Description = "If empty it'll be the same as email")]
            public string UserName { get; set; } = "";

            public string UserNameOrEmail { get { return string.IsNullOrWhiteSpace(UserName) ? Email : UserName; } }

            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string? Password { get; set; }

            public bool HasPassword { get { return !string.IsNullOrWhiteSpace(Password); } }

            public bool HasErrors(out string errorMessage) 
            {
                errorMessage = string.Empty;
                if (HasPassword) {
                    //if password is defined then username or email should be defined
                    if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(UserName))
                    {
                        errorMessage = "When using existing account, you need to state either user name, or email. If user name is left empty it'll be the same as email";
                        return true;
                    }
                }
                else
                {
                    //if password is not defined then user is creating new account and then Email is required and Username if empty will be same as email
                    if (string.IsNullOrWhiteSpace(Email)) 
                    {
                        errorMessage = "Email is required in case you're registering a new account.";
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
