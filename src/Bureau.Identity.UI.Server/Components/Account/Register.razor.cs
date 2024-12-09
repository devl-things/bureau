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
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class Register
    {
        private StatusMessageInput _statusMessage = new StatusMessageInput();

        [SupplyParameterFromForm]
        private InputModel _input { get; set; } = new();

        [SupplyParameterFromQuery(Name = BureauIdentityQueryParameterNames.ReturnUrl)]
        private string? _returnUrl { get; set; }
        [Inject]
        private IUserManager _userManager { get; set; } = default!;
        [Inject]
        private IAccountManager _accountManager { get; set; } = default!;

        [Inject]
        IEmailManager _emailManager { get; set; } = default!;

        [Inject]
        private ISignInManager _signInManager { get; init; }

        [Inject]
        private BureauRedirectManager _redirectManager { get; set; }
        [Inject]
        private ILogger<Register> _logger { get; set; }

        public async Task RegisterUser(EditContext editContext)
        {
            Result<BureauUser> userResult = await _userManager.AddUserAsync(_input.UserNameOrEmail, _input.Email, _input.Password);

            if (userResult.IsError)
            {
                SetMessage(userResult.Error);
                return;
            }
            BureauUser user = userResult.Value;

            _logger.Info(BureauIdentityMessages.UserCreatedWithPassword, user.UserName!);

            Result<string> codeResult = await _accountManager.GenerateEmailConfirmationTokenAsync(user);
            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeResult.Value));

            string callbackUrl = _redirectManager.BureauNavigationManager.GetUriForBasePageAddress(new ConfirmEmailPageAddress(user.Id, code, _returnUrl));

            await _emailManager.SendConfirmationLinkAsync(user, _input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (_accountManager.RequireConfirmedAccount)
            {
                _redirectManager.RedirectTo(new RegisterConfirmationPageAddress(_input.UserNameOrEmail, _returnUrl));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _redirectManager.RedirectTo(_returnUrl);
        }

        //TODO #12-2
        private void SetMessage(ResultError result)
        {
            //TODO
            _logger.Warning($"Error: {result.ErrorMessage}");
            _statusMessage.SetError($"Error: {result.ErrorMessage}");
        }
        //private void SetMessage(IdentityResult result)
        //{
        //    _statusMessage.SetError($"Error: {string.Join(",", result.Errors.Select(error => error.Description))}");
        //}

        //TODO #12-2
//        private bool TryCreateUser(out ApplicationUser user)
//        {
//            try
//            {
//                user = Activator.CreateInstance<ApplicationUser>();
//                return true;
//            }
//            catch
//            {
//                _statusMessage.SetError(BureauUIMessages.CriticalError);
//#pragma warning disable CA2017 // Parameter count mismatch
//                _logger.LogCritical(message: ErrorMessages.InstanceCreation, nameof(ApplicationUser));
//#pragma warning restore CA2017 // Parameter count mismatch
//                user = null!;
//                return false;
//            }
//        }

        //TODO #12-2
        //private bool TryGetEmailStore(out IUserEmailStore<ApplicationUser> emailStore)
        //{
        //    if (!_userManager.SupportsUserEmail)
        //    {
        //        _logger.LogCritical(message: BureauIdentityMessages.EmailStoreNotSupported);
        //        _statusMessage.SetError(BureauUIMessages.CriticalError);
        //        emailStore = null!;
        //        return false;
        //    }
        //    emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
        //    return true;
        //}

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
