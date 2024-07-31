using Bureau.UI.Web.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Bureau.UI.Web.Components.Account.Managers;
using Bureau.UI.Web.Components.Shared;
using Bureau.UI.Web.Components.Account.PageAddresses;
using Bureau.UI.Web.Utils;

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class Register
    {
        private StatusMessageInput _statusMessage = new StatusMessageInput();

        [SupplyParameterFromForm]
        private InputModel _input { get; set; } = new();

        [SupplyParameterFromQuery(Name = QueryParameterNames.ReturnUrl)]
        private string? _returnUrl { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Inject]
        private UserManager<ApplicationUser> _userManager { get; init; }
        [Inject]
        private IUserStore<ApplicationUser> _userStore { get; init; }
        [Inject]
        private SignInManager<ApplicationUser> _signInManager { get; init; }
        [Inject]
        private IEmailSender<ApplicationUser> _emailSender { get; init; }
        [Inject]
        private ILogger<Register> _logger { get; init; }
        [Inject]
        private IdentityRedirectManager _redirectManager { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


        public async Task RegisterUser(EditContext editContext)
        {
            ApplicationUser user;
            if (!TryCreateUser(out user)) 
            {
                return;
            }

            await _userStore.SetUserNameAsync(user, _input.UserNameOrEmail, CancellationToken.None);
            IUserEmailStore<ApplicationUser> emailStore;
            if (!TryGetEmailStore(out emailStore)) 
            {
                return;
            }

            await emailStore.SetEmailAsync(user, _input.Email, CancellationToken.None);

            IdentityResult result = await _userManager.CreateAsync(user, _input.Password);

            if (!result.Succeeded)
            {
                SetMessage(result);
                return;
            }

            _logger.Info(LogMessages.UserCreatedWithPassword, user.UserName!);

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string callbackUrl = _redirectManager.GetUriForBasePageAddress(new ConfirmEmailPageAddress(userId, code, _returnUrl));

            await _emailSender.SendConfirmationLinkAsync(user, _input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                _redirectManager.RedirectTo(new RegisterConfirmationPageAddress(_input.UserNameOrEmail, _returnUrl));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _redirectManager.RedirectTo(_returnUrl);
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
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = "";
            
            [MaxLength(256,ErrorMessage = "The {0} must be at max {1} characters long.")]
            [Display(Name = "User name", Description = "If empty it'll be the same as email")]
            public string UserName { get; set; } = "";

            public string UserNameOrEmail { get { return string.IsNullOrWhiteSpace(UserName) ? Email : UserName; } }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}
