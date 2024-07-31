using Bureau.UI.Web.Components.Account.Managers;
using Bureau.UI.Web.Components.Account.PageAddresses;
using Bureau.UI.Web.Components.Shared;
using Bureau.UI.Web.Data;
using Bureau.UI.Web.Utils;
using Bureau.UI.Web.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class ExternalLogin
    {
        private ExternalLoginInfo? _externalLoginInfo;
        private string? _providerDisplayName => _externalLoginInfo?.ProviderDisplayName;

        private StatusMessageInput _statusMessage = new StatusMessageInput();

        [SupplyParameterFromQuery(Name = QueryParameterNames.ReturnUrl)]
        private string? _returnUrl { get; set; }

        [SupplyParameterFromQuery(Name = QueryParameterNames.RemoteError)]
        private string? _remoteError { get; set; }

        [SupplyParameterFromQuery(Name = QueryParameterNames.Action)]
        private string? _action { get; set; }

        [CascadingParameter]
        private HttpContext _httpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel _input { get; set; } = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Inject]
        private SignInManager<ApplicationUser> _signInManager { get; init; }
        [Inject]
        private BureauUserManager _userManager { get; init; }
        [Inject]
        private IUserStore<ApplicationUser> _userStore { get; init; }
        [Inject]
        private IEmailSender<ApplicationUser> _emailSender { get; init; }
        [Inject]
        private IdentityRedirectManager _redirectManager { get; init; }
        [Inject]
        private ILogger<ExternalLogin> _logger { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        protected override async Task OnInitializedAsync()
        {
            if (_remoteError is not null)
            {
                _logger.LogError(LogMessages.ErrorFromExternalProvider, _remoteError);
                _redirectManager.RedirectToWithStatus(UriPages.Login, string.Format(LogMessages.ErrorFromExternalProvider, _remoteError), _httpContext);
            }

            _externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (_externalLoginInfo is null)
            {
                _logger.LogError(LogMessages.ErrorLoadingExternalLogin);
                _redirectManager.RedirectToWithStatus(UriPages.Login, LogMessages.ErrorLoadingExternalLogin, _httpContext);
            }

            if (HttpMethods.IsGet(_httpContext.Request.Method))
            {
                if (_action == CallbackActionNames.Login)
                {
                    await OnLoginCallbackAsync();
                    return;
                }

                // We should only reach this page via the login callback, so redirect back to
                // the login page if we get here some other way.
                _redirectManager.RedirectTo(UriPages.Login);
            }
        }

        private async Task OnLoginCallbackAsync()
        {
            // Sign in the user with this external login provider if the user already has a login.
            SignInResult result = await _signInManager.ExternalLoginSignInAsync(
                loginProvider: _externalLoginInfo?.LoginProvider!,
                providerKey: _externalLoginInfo?.ProviderKey!,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.Info(LogMessages.UserLoginInWithProvider,
                    _externalLoginInfo?.Principal.Identity?.Name,
                    _externalLoginInfo?.LoginProvider);
                _redirectManager.RedirectTo(_returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _redirectManager.RedirectTo(UriPages.Lockout);
            }

            // If the user does not have an account, then ask the user to create an account.
            if (TryGetEmail(out string email)) 
            {
                _input.Email = email;
            }
        }

        private async Task OnValidSubmitAsync()
        {
            if (_input.HasErrors(out string errorMessage)) 
            {
                _statusMessage.SetError(errorMessage);
                return;
            }
            ApplicationUser? user;
            if (_input.HasPassword)
            {
                user = await _userManager.FindByNameAsync(_input.UserNameOrEmail);
                if (user is null)
                {
                    _statusMessage.SetError(UIMessages.AccountDoesntExist);
                    return;
                }
                if (user.PasswordHash is null) 
                {
                    _statusMessage.SetWarning(UIMessages.ExistingUserByWrongPassword);
                    return;
                }

                if (TryGetEmail(out string email)) 
                {
                    ApplicationUser? userByEmail = await _userManager.FindByEmailAsync(email);
                    if (userByEmail is not null && user.Id != userByEmail.Id)
                    {
                        _logger.LogError(LogMessages.UserTriedDuplicatingAccount, _externalLoginInfo!.LoginProvider, _externalLoginInfo.ProviderKey, user.UserName, userByEmail.UserName);
                        _statusMessage.SetError(UIMessages.ExistingDoubleUser);
                        return;
                    }
                }

                SignInResult signInResult = await _signInManager.CheckPasswordSignInAsync(user, _input.Password, lockoutOnFailure: false);
                if (signInResult.RequiresTwoFactor)
                {
                    // TODO #6
                    _redirectManager.RedirectTo(new LoginWith2faPageAddress(_returnUrl!, false));
                }
                else if (signInResult.IsLockedOut)
                {
                    // TODO #5
                    _logger.Warning(LogMessages.UserLockedOut, _input.UserNameOrEmail);
                    _redirectManager.RedirectTo(UriPages.Lockout);
                }
                else if (!signInResult.Succeeded)
                {
                    _statusMessage.SetError(UIMessages.InvalidLogin);
                    return;
                }
            }
            else
            {
                if (!TryCreateUser(out user)) 
                {
                    return;
                }
                IUserEmailStore<ApplicationUser> emailStore;
                if (!TryGetEmailStore(out emailStore)) 
                {
                    return;
                }
                await _userStore.SetUserNameAsync(user, _input.UserNameOrEmail, CancellationToken.None);
                await emailStore.SetEmailAsync(user, _input.Email, CancellationToken.None);

                IdentityResult userCreationResult = await _userManager.CreateAsync(user);
                if (!userCreationResult.Succeeded)
                {
                    SetMessage(userCreationResult);
                    return;
                }
            }

            IdentityResult result = await _userManager.AddLoginAsync(user, _externalLoginInfo);
            if (!result.Succeeded) 
            {
                SetMessage(result);
                return;
            }
            _logger.Info(LogMessages.UserCreatedWithProvider, user.UserName, _externalLoginInfo.LoginProvider);

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string callbackUrl = _redirectManager.GetUriForBasePageAddress(new ConfirmEmailPageAddress(userId, code));

            await _emailSender.SendConfirmationLinkAsync(user, _input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                _redirectManager.RedirectTo(new RegisterConfirmationPageAddress(_input.UserNameOrEmail));
            }

            await _signInManager.SignInAsync(user, isPersistent: false, _externalLoginInfo.LoginProvider);
            _redirectManager.RedirectTo(_returnUrl);
        }


        private bool TryGetEmail(out string email) 
        {
            email = string.Empty;
            if (_externalLoginInfo!.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = _externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email)!;
                if (!string.IsNullOrWhiteSpace(email)) 
                {
                    return true;
                }
            }
            return false;
        }

        //TODO #12-2
        private void SetMessage(IdentityResult result)
        {
            _statusMessage.SetError($"Error: {string.Join(",", result.Errors.Select(error => error.Description))}");
        }

        //TODO #12-2
        private bool TryCreateUser(out ApplicationUser user)
        {
            try
            {
                user = Activator.CreateInstance<ApplicationUser>();
                return true;
            }
            catch
            {
                _statusMessage.SetError(UIMessages.CriticalError);
#pragma warning disable CA2017 // Parameter count mismatch
                _logger.LogCritical(message: LogMessages.InstanceCreation, nameof(ApplicationUser));
#pragma warning restore CA2017 // Parameter count mismatch
                user = null!;
                return false;
            }
        }

        //TODO #12-2
        private bool TryGetEmailStore(out IUserEmailStore<ApplicationUser> emailStore)
        {
            if (!_userManager.SupportsUserEmail)
            {
                _logger.LogCritical(message: LogMessages.EmailStoreNotSupported);
                _statusMessage.SetError(UIMessages.CriticalError);
                emailStore = null!;
                return false;
            }
            emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
            return true;
        }

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
