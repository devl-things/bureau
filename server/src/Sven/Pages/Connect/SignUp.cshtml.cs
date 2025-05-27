using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.ExternalLogins;
using Sven.PageModels.SignUp;
using Sven.Services;
using System.ComponentModel.DataAnnotations;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    public class SignUpModel : LocalizationPageModel<SignUpModel, SignUpModelText>, IExternalLoginProperty
    {
        private const string UNHANDABLE_ERROR_MESSAGE = "Something went wrong, try refreshing the page and start over.";

        [BindProperty(Name = AuthConstants.PropertyNames.Step)]
        public SignUpStep Step { get; set; } = SignUpStep.EnterEmail;

        [BindProperty(Name = AuthConstants.PropertyNames.Email)]
        [EmailAddress]
        [CustomValidation(typeof(SvenValidators), nameof(SvenValidators.ValidateEmailDomain))]
        public string? Email { get; set; }

        [BindProperty(Name = AuthConstants.PropertyNames.VerificationCode)]
        public string? VerificationCode { get; set; }

        [BindProperty(Name = AuthConstants.PropertyNames.Password)]
        public string? Password { get; set; }

        [BindProperty(Name = AuthConstants.PropertyNames.ConfirmPassword)]
        public string? ConfirmPassword { get; set; }
        public string Title { get; set; }
        public string? ErrorMessage { get; set; }

        private readonly SignUpModelText _text;
        private readonly ILogger<SignUpModel> _logger;
        private readonly ISymEncryptor _encryptor;
        private readonly INotificationService<VerificationCodeNotification> _notificationService;
        private readonly IUserProvider _userProvider;

        public SignUpModel(IStringLocalizer<Common> stringLocalizer,
            IStringLocalizer<SignUpModel> localizer,
            ILogger<SignUpModel> logger,
            ISymEncryptor encryptor,
            IUserProvider userProvider,
            INotificationService<VerificationCodeNotification> notificationService) : base(stringLocalizer, localizer)
        {
            _text = new SignUpModelText()
            {
                AlreadyAccount = _localizer[nameof(SignUpModelText.AlreadyAccount)],
                Create = _localizer[nameof(SignUpModelText.Create)],
                CreateAccountTitle = _localizer[nameof(SignUpModelText.CreateAccountTitle)],
                ConfirmPassword = _sharedLocalizer[nameof(SignUpModelText.ConfirmPassword)],
                Continue = _localizer[nameof(SignUpModelText.Continue)],
                Email = _sharedLocalizer[nameof(SignUpModelText.Email)],
                NeedNewCode = _localizer[nameof(SignUpModelText.NeedNewCode)],
                Or = _sharedLocalizer[nameof(SignUpModelText.Or)],
                Password = _sharedLocalizer[nameof(SignUpModelText.Password)],
                ResendCode = _sharedLocalizer[nameof(SignUpModelText.ReferenceEquals)],
                SetPasswordMessage = _localizer[nameof(SignUpModelText.SetPasswordMessage)],
                SignIn = _localizer[nameof(SignUpModelText.SignIn)],
                SignUpWith = _localizer[nameof(SignUpModelText.SignUpWith)],
                VerifyCode = _localizer[nameof(SignUpModelText.VerifyCode)],
                VerifyCodeMessage = _localizer[nameof(SignUpModelText.VerifyCodeMessage)],
            };
            Title = _text.CreateAccountTitle;
            ExternalLogins = ExternalLoginProviders.ExternalList;
            _logger = logger;
            _encryptor = encryptor;
            _userProvider = userProvider;
            _notificationService = notificationService;
        }
        public override SignUpModelText Text { get { return _text; } }

        public List<ExternalLoginModel> ExternalLogins { get; init; }

        public IActionResult OnGet([FromQuery(Name = AuthConstants.OAuth.FieldNames.Challenge)] string? challenge)
        {
            DecodeChallenge(challenge);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            switch (Step)
            {
                case SignUpStep.SetPassword:
                    return await OnPostSetPasswordAsync(cancellationToken);
                case SignUpStep.VerifyCode:
                    return await OnPostVerifyCodeAsync(cancellationToken);
                case SignUpStep.EnterEmail:
                default:
                    return await OnPostContinueAsync(cancellationToken);
            }
        }

        public async Task<IActionResult> OnPostContinueAsync(CancellationToken cancellationToken = default)
        {
            if (ValidateEmail() is { IsError: true } emailValidationResult)
            {
                return ReturnWithError(emailValidationResult.Error);
            }

            if (await _userProvider.ExistsUserWithEmailAsync(Email!, cancellationToken))
            {
                return ReturnWithError(new ResultError($"User tried to sign up with existing email ({Email})", "This email cannot be registered again."));
            }

            Result<string> codeResult = await _userProvider.GenerateVerificationCodeAsync(Email!, cancellationToken);
            if (codeResult.IsError)
            {
                return ReturnWithError(new ResultError(codeResult.Error, UNHANDABLE_ERROR_MESSAGE));
            }
            if (await SendVerificationEmailAsync(codeResult.Value, cancellationToken) is { IsError: true } codeSentResult)
            {
                return ReturnWithError(codeSentResult.Error);
            }
            return RedirectWithChallenge(SignUpStep.VerifyCode);
        }

        public async Task<IActionResult> OnPostVerifyCodeAsync(CancellationToken cancellationToken = default)
        {
            if (ValidateEmail() is { IsError: true } emailValidationResult)
            {
                return ReturnWithError(emailValidationResult.Error);
            }
            if (await VerifyCodeAsync(cancellationToken) is { IsError: true } codeVerifiedResult)
            {
                return ReturnWithError(codeVerifiedResult.Error);
            }

            return RedirectWithChallenge(SignUpStep.SetPassword);
        }

        public async Task<IActionResult> OnPostSetPasswordAsync(CancellationToken cancellationToken = default)
        {
            if (ValidateEmail() is { IsError: true } emailValidationResult)
            {
                return ReturnWithError(emailValidationResult.Error);
            }
            if (ValidatePasswords() is { IsError: true } validationResult)
            {
                return ReturnWithError(validationResult.Error);
            }

            Result<string> userCreatedResult = await _userProvider.CreateUserAsync(Email!, Password!, cancellationToken);
            if (userCreatedResult.IsError)
            {
                return ReturnWithError(new ResultError(userCreatedResult.Error, UNHANDABLE_ERROR_MESSAGE));
            }

            Result<string> ticketResult = await _userProvider.GenerateTicketAsync(userCreatedResult.Value, cancellationToken);
            if (userCreatedResult.IsError)
            {
                _logger.LogResultError(ticketResult.Error);
                return Redirect(Endpoints.Connect.SignIn);

            }
            return Redirect($"{Endpoints.Connect.SignIn}?{AuthConstants.PropertyNames.Mode}={PageModelTypes.SignIn.Ticket}&{AuthConstants.PropertyNames.Ticket}={ticketResult.Value}");
        }

        private Result ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return new ResultError("Email address is required.");
            }
            ModelStateEntry? entry;
            if (!ModelState.TryGetValue(AuthConstants.PropertyNames.Email, out entry) || entry.ValidationState != ModelValidationState.Valid)
            {
                return new ResultError(entry?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid email address.");
            }
            return true;
        }

        private Result ValidatePasswords()
        {
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                return new ResultError("Password cannot be empty.");
            }
            if (Password != ConfirmPassword)
            {
                return new ResultError("Confirmation password does not match with password.");
            }
            return true;
        }

        private PageResult ReturnWithError(ResultError error)
        {
            _logger.LogResultError(error);
            ErrorMessage = error.UserMessage;
            return Page();
        }

        private Result<string> EncodeChallenge(SignUpStep step)
        {
            return _encryptor.Encrypt([((int)step).ToString(), Email!]);
        }

        private void DecodeChallenge(string? challenge)
        {
            if (!string.IsNullOrWhiteSpace(challenge))
            {
                Result<string[]> valuesResult = _encryptor.DecryptStringArray(challenge);

                if (valuesResult.IsError)
                {
                    _logger.LogResultError(valuesResult.Error);
                }
                else if (valuesResult.Value.Length == 2)
                {
                    Step = (SignUpStep)int.Parse(valuesResult.Value[0]);
                    Email = valuesResult.Value[1];
                }
            }
        }

        private IActionResult RedirectWithChallenge(SignUpStep step)
        {
            Result<string> challengeResult = EncodeChallenge(step);
            if (challengeResult.IsError)
            {
                _logger.LogResultError(challengeResult.Error);
                ErrorMessage = UNHANDABLE_ERROR_MESSAGE;
                return Page();
            }
            return Redirect($"{Endpoints.Connect.SignUp}?{AuthConstants.OAuth.FieldNames.Challenge}={challengeResult.Value}");
        }

        private async Task<Result> SendVerificationEmailAsync(string code, CancellationToken cancellationToken)
        {
            VerificationCodeNotification notification = new()
            {
                Code = code,
                Email = Email!
            };
            Result notificationResult = await _notificationService.NotifyAsync(notification, cancellationToken);
            if (notificationResult.IsError)
            {
                return new ResultError(notificationResult.Error, "Email cannot be sent.");
            }
            return notificationResult;
        }

        private async Task<Result> VerifyCodeAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            if ("000000".Equals(VerificationCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
#endif
            if (string.IsNullOrWhiteSpace(VerificationCode) || !int.TryParse(VerificationCode, out int parsedCode)
                || parsedCode < _userProvider.MinVerificationCode || parsedCode > _userProvider.MaxVerificationCode)
            {
                return new ResultError("Invalid verification code.");
            }
            Result<string> verifyCodeResult = await _userProvider.GetVerificationCodeAsync(Email!, cancellationToken);
            if (verifyCodeResult.IsError)
            {
                return new ResultError(verifyCodeResult.Error, "Verification code not created.");
            }
            if (!VerificationCode.Equals(verifyCodeResult.Value, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultError("Incorrect verification code.");
            }
            return true;
        }
    }
}
