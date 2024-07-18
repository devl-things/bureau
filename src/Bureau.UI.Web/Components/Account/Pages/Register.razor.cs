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

namespace Bureau.UI.Web.Components.Account.Pages
{
    public partial class Register
    {
        private IEnumerable<IdentityError>? _identityErrors;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery(Name = "ReturnUrl")]
        private string? ReturnUrl { get; set; }

        private string? Message => _identityErrors is null ? null : $"Error: {string.Join(", ", _identityErrors.Select(error => error.Description))}";

        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; init; }
        [Inject]
        public required IUserStore<ApplicationUser> UserStore { get; init; }
        [Inject]
        public required SignInManager<ApplicationUser> SignInManager { get; init; }
        [Inject]
        public required IEmailSender<ApplicationUser> EmailSender { get; init; }
        [Inject]
        public required ILogger<Register> Logger { get; init; }
        [Inject]
        public required NavigationManager NavigationManager { get; init; }
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; init; }

        public async Task RegisterUser(EditContext editContext)
        {
            var user = CreateUser();
            string userName = string.IsNullOrWhiteSpace(Input.UserName) ? Input.Email : Input.UserName;
            await UserStore.SetUserNameAsync(user, userName, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                _identityErrors = result.Errors;
                return;
            }

            Logger.LogInformation("User created a new account with password.");

            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (UserManager.Options.SignIn.RequireConfirmedAccount)
            {
                RedirectManager.RedirectTo(
                    "Account/RegisterConfirmation",
                    new() { ["userName"] = userName, ["returnUrl"] = ReturnUrl });
            }

            await SignInManager.SignInAsync(user, isPersistent: false);
            RedirectManager.RedirectTo(ReturnUrl);
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
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
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = "";
            
            [MaxLength(256,ErrorMessage = "The {0} must be at max {1} characters long.")]
            [Display(Name = "User name", Description = "If empty it'll be the same as email")]
            public string UserName { get; set; } = "";

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
