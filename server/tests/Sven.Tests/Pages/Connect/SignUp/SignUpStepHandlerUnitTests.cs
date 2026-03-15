using Bureau;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sven.Configurations;
using Sven;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.PageModels.Connect.SignUp;
using Sven.Services;

namespace Sven.Tests.Pages.Connect.SignUp
{
    public class SignUpStepHandlerUnitTests
    {
        private const string TestEmail = "test.user@example.local";
        private const string TestPassword = "P@ssw0rd!";
        private const string MismatchedPassword = "doesnt-match";
        private const string TestChallengeId = "challenge-id-abc123";
        private const string TestUserId = "user-id-xyz789";
        private const string TestVerificationCode = "123456";
        private const string WrongVerificationCode = "654321";
        private const string TestEncryptedEmail = "encrypted-email-token";
        private const string ErrorCodeGeneric = "error-code";
        private const string TestRequestPath = "/connect/signup";

        // --- Context builder ---

        private static (SignUpStepContext Context, Func<ResultError?> GetCapturedError) BuildContext(
            SignUpStep step = SignUpStep.EnterEmail,
            string? challenge = null,
            StepModelRequest? stepModel = null,
            Action<DefaultHttpContext>? configureHttp = null,
            IReadOnlyDictionary<string, string>? formValues = null)
        {
            ResultError? capturedError = null;
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString(TestRequestPath);
            Dictionary<string, StringValues> formDict = new Dictionary<string, StringValues>();
            if (formValues != null)
            {
                foreach (KeyValuePair<string, string> kv in formValues)
                {
                    formDict[kv.Key] = new StringValues(kv.Value);
                }
            }
            httpContext.Request.Form = new FormCollection(formDict);
            configureHttp?.Invoke(httpContext);

            StepChallengeRequest stepChallenge = new StepChallengeRequest(step);
            stepChallenge.Challenge = challenge;
            SignUpViewModel viewModel = new SignUpViewModel();
            SignUpStepContext context = new SignUpStepContext(
                stepChallenge: stepChallenge,
                stepModel: stepModel,
                viewModel: viewModel,
                pageFactory: () => new PageResult(),
                pageWithErrorFactory: err => { capturedError = err; return new PageResult(); },
                redirectFactory: url => new RedirectResult(url),
                request: httpContext.Request);
            return (context, () => capturedError);
        }

        private static ErrorTranslations BuildErrorTranslations()
        {
            IStringLocalizer<Resources.Errors> localizer = Substitute.For<IStringLocalizer<Resources.Errors>>();
            localizer[Arg.Any<string>()].Returns(callInfo =>
                new LocalizedString((string)callInfo[0], (string)callInfo[0]));
            return new ErrorTranslations(localizer);
        }

        private static SignUpFlowHelper BuildSignUpFlowHelper(IUserService userService)
        {
            ILogger<SignUpFlowHelper> logger = Substitute.For<ILogger<SignUpFlowHelper>>();
            return new SignUpFlowHelper(logger, userService);
        }

        private static ConnectTranslations BuildConnectTranslations()
        {
            IStringLocalizer<Resources.Common> commonLocalizer = Substitute.For<IStringLocalizer<Resources.Common>>();
            commonLocalizer[Arg.Any<string>()].Returns(callInfo =>
                new LocalizedString((string)callInfo[0], (string)callInfo[0]));
            IStringLocalizer<Resources.Pages.Connect> connectLocalizer = Substitute.For<IStringLocalizer<Resources.Pages.Connect>>();
            connectLocalizer[Arg.Any<string>()].Returns(callInfo =>
                new LocalizedString((string)callInfo[0], (string)callInfo[0]));
            return new ConnectTranslations(commonLocalizer, connectLocalizer);
        }

        private static UserVerificationCode BuildVerificationCode(
            string email = TestEmail,
            string code = TestVerificationCode,
            string? userId = null,
            VerificationStatus status = VerificationStatus.EmailSent)
        {
            return new UserVerificationCode(email, code, userId, status, DateTimeOffset.UtcNow.AddMinutes(10))
            {
                Id = TestChallengeId
            };
        }

        // --- PlainEnterEmailStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainEnterEmailStepHandler_HandleGetAsync_ReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainEnterEmailStepHandler handler = new PlainEnterEmailStepHandler(
                Substitute.For<ILogger<PlainEnterEmailStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            (SignUpStepContext context, Func<ResultError?> _) = BuildContext();

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainEnterEmailStepHandler_HandlePostAsync_ExistingEmail_ReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.ExistsUserWithEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainEnterEmailStepHandler handler = new PlainEnterEmailStepHandler(
                Substitute.For<ILogger<PlainEnterEmailStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail };
            (SignUpStepContext context, Func<ResultError?> getCapturedError) = BuildContext(stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            Assert.IsType<PageResult>(result);
            Assert.NotNull(getCapturedError());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainEnterEmailStepHandler_HandlePostAsync_NewEmail_RedirectsToVerifyCode()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.ExistsUserWithEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
            UserVerificationCode verificationCode = BuildVerificationCode();
            userProvider.GenerateVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdateVerificationCodeStatusAsync(Arg.Any<string>(), Arg.Any<VerificationStatus>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            notificationService.NotifyAsync(Arg.Any<UserVerificationCodeNotification>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            PlainEnterEmailStepHandler handler = new PlainEnterEmailStepHandler(
                Substitute.For<ILogger<PlainEnterEmailStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail };
            (SignUpStepContext context, Func<ResultError?> _) = BuildContext(stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.VerifyCode.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
        }

        // --- PlainVerifyCodeStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainVerifyCodeStepHandler_HandleGetAsync_ValidChallenge_ReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            UserVerificationCode verificationCode = BuildVerificationCode();
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainVerifyCodeStepHandler handler = new PlainVerifyCodeStepHandler(
                Substitute.For<ILogger<PlainVerifyCodeStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.VerifyCode, challenge: TestChallengeId);

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainVerifyCodeStepHandler_HandleGetAsync_InvalidChallenge_RedirectsToSignIn()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ResultError.From(ErrorCodeGeneric));
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainVerifyCodeStepHandler handler = new PlainVerifyCodeStepHandler(
                Substitute.For<ILogger<PlainVerifyCodeStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.VerifyCode, challenge: TestChallengeId);

            IActionResult result = await handler.HandleGetAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Connect.SignIn, redirect.Url);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainVerifyCodeStepHandler_HandlePostAsync_InvalidCode_ReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.MinVerificationCode.Returns(100000);
            userProvider.MaxVerificationCode.Returns(999999);
            userProvider.ExistsUserWithEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
            UserVerificationCode verificationCode = BuildVerificationCode(code: TestVerificationCode, userId: null);
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainVerifyCodeStepHandler handler = new PlainVerifyCodeStepHandler(
                Substitute.For<ILogger<PlainVerifyCodeStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail, VerificationCode = WrongVerificationCode };
            (SignUpStepContext context, Func<ResultError?> getCapturedError) =
                BuildContext(step: SignUpStep.VerifyCode, challenge: TestChallengeId, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            Assert.IsType<PageResult>(result);
            Assert.NotNull(getCapturedError());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainVerifyCodeStepHandler_HandlePostAsync_ValidCode_RedirectsToSetPassword()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.MinVerificationCode.Returns(100000);
            userProvider.MaxVerificationCode.Returns(999999);
            userProvider.ExistsUserWithEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
            UserVerificationCode verificationCode = BuildVerificationCode(code: TestVerificationCode, userId: null);
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdateVerificationCodeStatusAsync(Arg.Any<string>(), Arg.Any<VerificationStatus>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            PlainVerifyCodeStepHandler handler = new PlainVerifyCodeStepHandler(
                Substitute.For<ILogger<PlainVerifyCodeStepHandler>>(),
                userProvider,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail, VerificationCode = TestVerificationCode };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.VerifyCode, challenge: TestChallengeId, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.SetPassword.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
        }

        // --- PlainSetPasswordStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSetPasswordStepHandler_HandlePostAsync_PasswordMismatch_ReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            PlainSetPasswordStepHandler handler = new PlainSetPasswordStepHandler(
                Substitute.For<ILogger<PlainSetPasswordStepHandler>>(),
                userProvider,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Password = TestPassword, ConfirmPassword = MismatchedPassword };
            (SignUpStepContext context, Func<ResultError?> getCapturedError) =
                BuildContext(step: SignUpStep.SetPassword, challenge: TestChallengeId, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            Assert.IsType<PageResult>(result);
            Assert.NotNull(getCapturedError());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSetPasswordStepHandler_HandlePostAsync_ValidPassword_RedirectsToFinalMessage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            UserVerificationCode verificationCode = BuildVerificationCode(status: VerificationStatus.Verified);
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<string>(TestUserId));
            PlainSetPasswordStepHandler handler = new PlainSetPasswordStepHandler(
                Substitute.For<ILogger<PlainSetPasswordStepHandler>>(),
                userProvider,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Password = TestPassword, ConfirmPassword = TestPassword };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.SetPassword, challenge: TestChallengeId, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.FinalMessage.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
        }

        // --- ForgotEnterEmailStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ForgotEnterEmailStepHandler_HandlePostAsync_KnownEmail_RedirectsToCodeSent()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            ISymEncryptor encryptor = Substitute.For<ISymEncryptor>();
            encryptor.Encrypt(Arg.Any<string>()).Returns(new Result<string>(TestEncryptedEmail));
            UserVerificationCode verificationCode = BuildVerificationCode(userId: TestUserId);
            userProvider.GenerateVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdateVerificationCodeStatusAsync(Arg.Any<string>(), Arg.Any<VerificationStatus>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            INotificationService<PasswordResetNotification> notificationService =
                Substitute.For<INotificationService<PasswordResetNotification>>();
            notificationService.NotifyAsync(Arg.Any<PasswordResetNotification>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            ForgotEnterEmailStepHandler handler = new ForgotEnterEmailStepHandler(
                Substitute.For<ILogger<ForgotEnterEmailStepHandler>>(),
                userProvider,
                encryptor,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail };
            (SignUpStepContext context, Func<ResultError?> _) = BuildContext(stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.CodeSent.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ForgotEnterEmailStepHandler_HandlePostAsync_UnknownEmail_RedirectsToCodeSent()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            ISymEncryptor encryptor = Substitute.For<ISymEncryptor>();
            encryptor.Encrypt(Arg.Any<string>()).Returns(new Result<string>(TestEncryptedEmail));
            UserVerificationCode verificationCode = BuildVerificationCode(userId: null);
            userProvider.GenerateVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdateVerificationCodeStatusAsync(Arg.Any<string>(), Arg.Any<VerificationStatus>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            INotificationService<PasswordResetNotification> notificationService =
                Substitute.For<INotificationService<PasswordResetNotification>>();
            ForgotEnterEmailStepHandler handler = new ForgotEnterEmailStepHandler(
                Substitute.For<ILogger<ForgotEnterEmailStepHandler>>(),
                userProvider,
                encryptor,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail };
            (SignUpStepContext context, Func<ResultError?> _) = BuildContext(stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.CodeSent.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
        }

        // --- ForgotCodeSentStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ForgotCodeSentStepHandler_HandleGetAsync_ValidChallenge_DecryptsEmailAndReturnsPage()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            ISymEncryptor encryptor = Substitute.For<ISymEncryptor>();
            encryptor.DecryptString(Arg.Any<string>()).Returns(new Result<string>(TestEmail));
            INotificationService<PasswordResetNotification> notificationService =
                Substitute.For<INotificationService<PasswordResetNotification>>();
            ForgotCodeSentStepHandler handler = new ForgotCodeSentStepHandler(
                Substitute.For<ILogger<ForgotCodeSentStepHandler>>(),
                userProvider,
                encryptor,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.CodeSent, challenge: TestEncryptedEmail);

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
            Assert.Equal(TestEmail, context.ViewModel.Email);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ForgotCodeSentStepHandler_HandlePostAsync_Resend_GeneratesNewCode()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            ISymEncryptor encryptor = Substitute.For<ISymEncryptor>();
            encryptor.DecryptString(Arg.Any<string>()).Returns(new Result<string>(TestEmail));
            UserVerificationCode verificationCode = BuildVerificationCode(userId: TestUserId);
            userProvider.GenerateVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdateVerificationCodeStatusAsync(Arg.Any<string>(), Arg.Any<VerificationStatus>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            INotificationService<PasswordResetNotification> notificationService =
                Substitute.For<INotificationService<PasswordResetNotification>>();
            notificationService.NotifyAsync(Arg.Any<PasswordResetNotification>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            ForgotCodeSentStepHandler handler = new ForgotCodeSentStepHandler(
                Substitute.For<ILogger<ForgotCodeSentStepHandler>>(),
                userProvider,
                encryptor,
                notificationService,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Email = TestEmail };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.CodeSent, challenge: TestEncryptedEmail, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            Assert.IsType<PageResult>(result);
            await userProvider.Received(1).GenerateVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        // --- ForgotSetPasswordStepHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ForgotSetPasswordStepHandler_HandlePostAsync_ValidChallenge_UpdatesPasswordAndRedirects()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            UserVerificationCode verificationCode = BuildVerificationCode(userId: TestUserId, status: VerificationStatus.EmailSent);
            userProvider.GetVerificationCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<UserVerificationCode>(verificationCode));
            userProvider.UpdatePasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result());
            ForgotSetPasswordStepHandler handler = new ForgotSetPasswordStepHandler(
                Substitute.For<ILogger<ForgotSetPasswordStepHandler>>(),
                userProvider,
                BuildSignUpFlowHelper(userProvider),
                BuildErrorTranslations());
            StepModelRequest stepModel = new StepModelRequest { Password = TestPassword, ConfirmPassword = TestPassword };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.SetPassword, challenge: TestChallengeId, stepModel: stepModel);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Contains(SignUpStep.FinalMessage.ToCode(), redirect.Url, StringComparison.OrdinalIgnoreCase);
            await userProvider.Received(1).UpdatePasswordAsync(TestUserId, TestPassword, Arg.Any<CancellationToken>());
        }

        // --- PlainSignUpFlowDispatcher ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignUpFlowDispatcher_HandleGetAsync_EnterEmailStep_ReturnsPage()
        {
            PlainSignUpFlowDispatcher dispatcher = BuildPlainSignUpFlowDispatcher();
            (SignUpStepContext context, Func<ResultError?> _) = BuildContext(step: SignUpStep.EnterEmail);

            IActionResult result = await dispatcher.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignUpFlowDispatcher_HandlePostAsync_StepMismatch_ReturnsBadRequest()
        {
            PlainSignUpFlowDispatcher dispatcher = BuildPlainSignUpFlowDispatcher();
            StepModelRequest stepModel = new StepModelRequest { Step = SignUpStep.VerifyCode };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.EnterEmail, stepModel: stepModel);

            IActionResult result = await dispatcher.HandlePostAsync(context);

            StatusCodeResult statusCode = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCode.StatusCode);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignUpFlowDispatcher_HandlePostAsync_FinalMessageStep_Returns405()
        {
            PlainSignUpFlowDispatcher dispatcher = BuildPlainSignUpFlowDispatcher();
            StepModelRequest stepModel = new StepModelRequest { Step = SignUpStep.FinalMessage };
            (SignUpStepContext context, Func<ResultError?> _) =
                BuildContext(step: SignUpStep.FinalMessage, stepModel: stepModel);

            IActionResult result = await dispatcher.HandlePostAsync(context);

            StatusCodeResult statusCode = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status405MethodNotAllowed, statusCode.StatusCode);
        }

        // --- Helper ---

        private static PlainSignUpFlowDispatcher BuildPlainSignUpFlowDispatcher()
        {
            IUserService userProvider = Substitute.For<IUserService>();
            INotificationService<UserVerificationCodeNotification> notificationService =
                Substitute.For<INotificationService<UserVerificationCodeNotification>>();
            SignUpFlowHelper flowHelper = BuildSignUpFlowHelper(userProvider);
            ErrorTranslations errorTranslations = BuildErrorTranslations();

            PlainEnterEmailStepHandler enterEmailHandler = new PlainEnterEmailStepHandler(
                Substitute.For<ILogger<PlainEnterEmailStepHandler>>(),
                userProvider, notificationService, flowHelper, errorTranslations);
            PlainVerifyCodeStepHandler verifyCodeHandler = new PlainVerifyCodeStepHandler(
                Substitute.For<ILogger<PlainVerifyCodeStepHandler>>(),
                userProvider, notificationService, flowHelper, errorTranslations);
            PlainSetPasswordStepHandler setPasswordHandler = new PlainSetPasswordStepHandler(
                Substitute.For<ILogger<PlainSetPasswordStepHandler>>(),
                userProvider, flowHelper, errorTranslations);
            PlainFinalMessageStepHandler finalMessageHandler = new PlainFinalMessageStepHandler(
                Substitute.For<ILogger<PlainFinalMessageStepHandler>>());

            IExternalProviderRegistry registry = Substitute.For<IExternalProviderRegistry>();
            registry.Providers.Returns(new List<RegisteredExternalProvider>().AsReadOnly());

            return new PlainSignUpFlowDispatcher(
                Substitute.For<ILogger<PlainSignUpFlowDispatcher>>(),
                enterEmailHandler,
                verifyCodeHandler,
                setPasswordHandler,
                finalMessageHandler,
                BuildConnectTranslations(),
                registry);
        }
    }
}
