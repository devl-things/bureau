using Bureau.UI.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Bureau.UI.Web.Components.Account.Shared;
using Bureau.UI.Web.Components.Shared;
using Bureau.UI.Web.Validation;

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class ExternalLogin
    {
        public const string LoginCallbackAction = "LoginCallback";
                
        private ExternalLoginInfo _externalLoginInfo = default!;
        private string? ProviderDisplayName => _externalLoginInfo.ProviderDisplayName;

        private StatusMessageInput StatusMessage = new StatusMessageInput();

        [SupplyParameterFromQuery(Name = "ReturnUrl")]
        private string? _returnUrl { get; set; }

        [SupplyParameterFromQuery(Name = "RemoteError")]
        private string? _remoteError { get; set; }

        [SupplyParameterFromQuery(Name = "Action")]
        private string? _action { get; set; }

        [CascadingParameter]
        private HttpContext _httpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [Inject]
        public required SignInManager<ApplicationUser> SignInManager { get; init; }
        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; init; }
        [Inject]
        public required IUserStore<ApplicationUser> UserStore { get; init; }
        [Inject]
        public required IEmailSender<ApplicationUser> EmailSender { get; init; }
        [Inject]
        public required NavigationManager NavigationManager { get; init; }
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; init; }
        [Inject]
        public required ILogger<ExternalLogin> Logger { get; init; }

        protected override async Task OnInitializedAsync()
        {
            if (_remoteError is not null)
            {
                RedirectManager.RedirectToWithStatus("Account/Login", $"Error from external provider: {_remoteError}", _httpContext);
            }

            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information.", _httpContext);
            }

            _externalLoginInfo = info;

            if (HttpMethods.IsGet(_httpContext.Request.Method))
            {
                if (_action == LoginCallbackAction)
                {
                    await OnLoginCallbackAsync();
                    return;
                }

                // We should only reach this page via the login callback, so redirect back to
                // the login page if we get here some other way.
                RedirectManager.RedirectTo("Account/Login");
            }
        }

        private async Task OnLoginCallbackAsync()
        {
            // Sign in the user with this external login provider if the user already has a login.
            var result = await SignInManager.ExternalLoginSignInAsync(
                _externalLoginInfo.LoginProvider,
                _externalLoginInfo.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                Logger.LogInformation(
                    "{Name} logged in with {LoginProvider} provider.",
                    _externalLoginInfo.Principal.Identity?.Name,
                    _externalLoginInfo.LoginProvider);
                RedirectManager.RedirectTo(_returnUrl);
            }
            else if (result.IsLockedOut)
            {
                RedirectManager.RedirectTo("Account/Lockout");
            }

            // If the user does not have an account, then ask the user to create an account.
            if (_externalLoginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input.Email = _externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            }
        }

        private async Task OnValidSubmitAsync()
        {
            if (Input.HasErrors(out string errorMessage)) 
            {
                StatusMessage.SetError(errorMessage);
                return;
            }
            ApplicationUser? user;
            if (Input.HasPassword)
            {
                user = await UserManager.FindByNameAsync(Input.UserNameOrEmail);
                if (user is null)
                {
                    StatusMessage.SetError("Such account doesn't exist. Either omit password and create new one, or insert correct username/password combination.");
                    return;
                }
                if (user.PasswordHash is null) 
                {
                    StatusMessage.SetWarning(@"There is an existing account, however, that account has no password set. 
    Please log in to that account, with the existing external login method, and set a password. 
    Then you can associate it with additional external login methods.");
                    return;
                }
                SignInResult signInResult = await SignInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                if (signInResult.RequiresTwoFactor)
                {
                    // TODO #6
                    RedirectManager.RedirectTo(
                        "Account/LoginWith2fa",
                        new() { ["returnUrl"] = _returnUrl, ["rememberMe"] = false });
                }
                else if (signInResult.IsLockedOut)
                {
                    // TODO #5
                    Logger.LogWarning("User account locked out.");
                    RedirectManager.RedirectTo("Account/Lockout");
                }
                else if (!signInResult.Succeeded)
                {
                    StatusMessage.SetError("Error: Invalid login attempt.");
                    return;
                }
            }
            else
            {
                user = CreateUserInstance();
                IUserEmailStore<ApplicationUser> emailStore = GetEmailStore();
                await UserStore.SetUserNameAsync(user, Input.UserNameOrEmail, CancellationToken.None);
                await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                IdentityResult userCreationResult = await UserManager.CreateAsync(user);
                if (!userCreationResult.Succeeded)
                {
                    SetMessage(userCreationResult);
                    return;
                }
            }

            IdentityResult result = await UserManager.AddLoginAsync(user, _externalLoginInfo);
            if (!result.Succeeded) 
            {
                SetMessage(result);
                return;
            }
            Logger.LogInformation("User created an account using {Name} provider.", _externalLoginInfo.LoginProvider);

            string userId = await UserManager.GetUserIdAsync(user);
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
            await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (UserManager.Options.SignIn.RequireConfirmedAccount)
            {
                RedirectManager.RedirectTo("Account/RegisterConfirmation", new() { ["userName"] = Input.UserNameOrEmail });
            }

            await SignInManager.SignInAsync(user, isPersistent: false, _externalLoginInfo.LoginProvider);
            RedirectManager.RedirectTo(_returnUrl);
        }

        private void SetMessage(IdentityResult result)
        {
            StatusMessage.SetError($"Error: {string.Join(",", result.Errors.Select(error => error.Description))}");
        }

        private ApplicationUser CreateUserInstance()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
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
