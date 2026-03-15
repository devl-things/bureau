using Bureau;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sven.Configurations;
using Sven;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.Connect.SignIn;
using Sven.Services;
using System.Security.Claims;

namespace Sven.Tests.Pages.Connect.SignIn
{
    public class SignInHandlerUnitTests
    {
        private const string TestUserId = "user1";
        private const string TestUsername = "user";
        private const string TestPassword = "pass";
        private const string WrongPassword = "wrong";
        private const string TestAuthScheme = "test";
        private const string TestPkceKey = "valid-pkce-key";
        private const string TestTicketValid = "valid-ticket";
        private const string TestTicketInvalid = "invalid-ticket";
        private const string ErrorCodeInvalid = "invalid";
        private const string ErrorCodeNotFound = "not found";

        private static ErrorTranslations BuildErrorTranslations()
        {
            IStringLocalizer<Resources.Errors> localizer = Substitute.For<IStringLocalizer<Resources.Errors>>();
            localizer[Arg.Any<string>()].Returns(callInfo =>
                new LocalizedString((string)callInfo[0], (string)callInfo[0]));
            return new ErrorTranslations(localizer);
        }

        private static SignInContext BuildContext(
            SignInMode mode = SignInMode.Plain,
            LoginCredentialsRequest? credentials = null,
            Action<DefaultHttpContext>? configureHttpContext = null)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            IAuthenticationService authService = Substitute.For<IAuthenticationService>();
            authService.SignInAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<ClaimsPrincipal>(), Arg.Any<AuthenticationProperties>())
                .Returns(Task.CompletedTask);
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(authService);
            httpContext.RequestServices = services.BuildServiceProvider();
            configureHttpContext?.Invoke(httpContext);

            return new SignInContext(
                mode: mode,
                pageFactory: () => new PageResult(),
                redirectFactory: url => new RedirectResult(url),
                statusCodeFactory: code => new StatusCodeResult(code),
                request: httpContext.Request,
                httpContext: httpContext,
                credentials: credentials);
        }

        // --- PlainSignInHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignIn_HandleGetAsync_ReturnsPage()
        {
            ILogger<PlainSignInHandler> logger = Substitute.For<ILogger<PlainSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            PlainSignInHandler handler = new PlainSignInHandler(logger, claimsProvider, BuildErrorTranslations());
            SignInContext context = BuildContext();

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignIn_HandlePostAsync_InvalidCredentials_ReturnsPageWithError()
        {
            ILogger<PlainSignInHandler> logger = Substitute.For<ILogger<PlainSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            claimsProvider.GetClaimsPrincipalAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ResultError.From(ErrorCodeInvalid));
            PlainSignInHandler handler = new PlainSignInHandler(logger, claimsProvider, BuildErrorTranslations());
            LoginCredentialsRequest credentials = new LoginCredentialsRequest { Username = TestUsername, Password = WrongPassword };
            SignInContext context = BuildContext(SignInMode.Plain, credentials);

            IActionResult result = await handler.HandlePostAsync(context);

            Assert.IsType<PageResult>(result);
            Assert.False(string.IsNullOrWhiteSpace(context.ErrorMessage));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PlainSignIn_HandlePostAsync_ValidCredentials_RedirectsToAccount()
        {
            ILogger<PlainSignInHandler> logger = Substitute.For<ILogger<PlainSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, TestUserId) }, TestAuthScheme));
            claimsProvider.GetClaimsPrincipalAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<ClaimsPrincipal>(principal));
            PlainSignInHandler handler = new PlainSignInHandler(logger, claimsProvider, BuildErrorTranslations());
            LoginCredentialsRequest credentials = new LoginCredentialsRequest { Username = TestUsername, Password = TestPassword };
            SignInContext context = BuildContext(SignInMode.Plain, credentials);

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Account.AccountInfo, redirect.Url);
        }

        // --- PkceSignInHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PkceSignIn_HandleGetAsync_NoPkceCookie_ReturnsOAuthError()
        {
            ILogger<PkceSignInHandler> logger = Substitute.For<ILogger<PkceSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            AuthCodeService authCodeProvider = BuildAuthCodeService(pkceKeyExists: false);
            PkceSignInHandler handler = new PkceSignInHandler(logger, claimsProvider, authCodeProvider, BuildErrorTranslations());
            SignInContext context = BuildContext(SignInMode.Pkce);

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsNotType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PkceSignIn_HandleGetAsync_ValidPkceCookie_ReturnsPage()
        {
            ILogger<PkceSignInHandler> logger = Substitute.For<ILogger<PkceSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            AuthCodeService authCodeProvider = BuildAuthCodeService(pkceKey: TestPkceKey, pkceKeyExists: true);
            PkceSignInHandler handler = new PkceSignInHandler(logger, claimsProvider, authCodeProvider, BuildErrorTranslations());
            SignInContext context = BuildContext(SignInMode.Pkce, configureHttpContext: ctx =>
            {
                ctx.Request.Headers.Cookie = $"{AuthConstants.CookieNames.PkceKey}={TestPkceKey}";
            });

            IActionResult result = await handler.HandleGetAsync(context);

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task PkceSignIn_HandlePostAsync_ValidCredentials_RedirectsToAuthorize()
        {
            ILogger<PkceSignInHandler> logger = Substitute.For<ILogger<PkceSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, TestUserId) }, TestAuthScheme));
            claimsProvider.GetClaimsPrincipalAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<ClaimsPrincipal>(principal));
            AuthCodeService authCodeProvider = BuildAuthCodeService(pkceKey: TestPkceKey, pkceKeyExists: true);
            PkceSignInHandler handler = new PkceSignInHandler(logger, claimsProvider, authCodeProvider, BuildErrorTranslations());
            LoginCredentialsRequest credentials = new LoginCredentialsRequest { Username = TestUsername, Password = TestPassword };
            SignInContext context = BuildContext(SignInMode.Pkce, credentials, ctx =>
            {
                ctx.Request.Headers.Cookie = $"{AuthConstants.CookieNames.PkceKey}={TestPkceKey}";
            });

            IActionResult result = await handler.HandlePostAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Connect.AuthorizePkce, redirect.Url);
        }

        // --- TicketSignInHandler ---

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TicketSignIn_HandleGetAsync_NoTicket_RedirectsToSignIn()
        {
            ILogger<TicketSignInHandler> logger = Substitute.For<ILogger<TicketSignInHandler>>();
            IUserService userProvider = Substitute.For<IUserService>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            TicketSignInHandler handler = new TicketSignInHandler(logger, userProvider, claimsProvider);
            SignInContext context = BuildContext(SignInMode.Ticket);

            IActionResult result = await handler.HandleGetAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Connect.SignIn, redirect.Url);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TicketSignIn_HandleGetAsync_InvalidTicket_RedirectsToSignIn()
        {
            ILogger<TicketSignInHandler> logger = Substitute.For<ILogger<TicketSignInHandler>>();
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.GetUserIdByTicketAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ResultError.From(ErrorCodeNotFound));
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            TicketSignInHandler handler = new TicketSignInHandler(logger, userProvider, claimsProvider);
            SignInContext context = BuildContext(SignInMode.Ticket, configureHttpContext: ctx =>
            {
                ctx.Request.QueryString = new QueryString($"?{AuthConstants.PropertyNames.Ticket}={TestTicketInvalid}");
            });

            IActionResult result = await handler.HandleGetAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Connect.SignIn, redirect.Url);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TicketSignIn_HandleGetAsync_ValidTicket_RedirectsToAccount()
        {
            ILogger<TicketSignInHandler> logger = Substitute.For<ILogger<TicketSignInHandler>>();
            IUserService userProvider = Substitute.For<IUserService>();
            userProvider.GetUserIdByTicketAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<string>(TestUserId));
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, TestUserId) }, TestAuthScheme));
            claimsProvider.GetClaimsPrincipalAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new Result<ClaimsPrincipal>(principal));
            TicketSignInHandler handler = new TicketSignInHandler(logger, userProvider, claimsProvider);
            SignInContext context = BuildContext(SignInMode.Ticket, configureHttpContext: ctx =>
            {
                ctx.Request.QueryString = new QueryString($"?{AuthConstants.PropertyNames.Ticket}={TestTicketValid}");
            });

            IActionResult result = await handler.HandleGetAsync(context);

            RedirectResult redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(Endpoints.Account.AccountInfo, redirect.Url);
        }

        // --- SignInHandlerFactory ---

        [Fact]
        [Trait("Category", "Unit")]
        public void SignInHandlerFactory_Ticket_HasNoPostHandler()
        {
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            SignInHandlerFactory factory = new SignInHandlerFactory(serviceProvider);

            bool result = factory.TryGetPostHandler(SignInMode.Ticket, out ISignInPostHandler? postHandler);

            Assert.False(result);
            Assert.Null(postHandler);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SignInHandlerFactory_Plain_HasPostHandler()
        {
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            PlainSignInHandler plainHandler = BuildPlainSignInHandler();
            serviceProvider.GetService(typeof(PlainSignInHandler)).Returns(plainHandler);
            SignInHandlerFactory factory = new SignInHandlerFactory(serviceProvider);

            bool result = factory.TryGetPostHandler(SignInMode.Plain, out ISignInPostHandler? postHandler);

            Assert.True(result);
            Assert.NotNull(postHandler);
            Assert.IsType<PlainSignInHandler>(postHandler);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SignInHandlerFactory_Pkce_HasPostHandler()
        {
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            PkceSignInHandler pkceHandler = BuildPkceSignInHandler();
            serviceProvider.GetService(typeof(PkceSignInHandler)).Returns(pkceHandler);
            SignInHandlerFactory factory = new SignInHandlerFactory(serviceProvider);

            bool result = factory.TryGetPostHandler(SignInMode.Pkce, out ISignInPostHandler? postHandler);

            Assert.True(result);
            Assert.NotNull(postHandler);
            Assert.IsType<PkceSignInHandler>(postHandler);
        }

        // --- Helpers ---

        private static AuthCodeService BuildAuthCodeService(string pkceKey = TestPkceKey, bool pkceKeyExists = false)
        {
            InMemoryStore<string, AuthCode> authCodeStore = new InMemoryStore<string, AuthCode>();
            IStore<string, OAuthRequest> pkceRequestStore = Substitute.For<IStore<string, OAuthRequest>>();
            pkceRequestStore.Exists(pkceKey).Returns(pkceKeyExists);
            IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions
            {
                AuthorizationCodeLifetime = TimeSpan.FromMinutes(5)
            });
            ILogger<AuthCodeService> logger = Substitute.For<ILogger<AuthCodeService>>();
            return new AuthCodeService(authOptions, TimeProvider.System, pkceRequestStore, authCodeStore, logger);
        }

        private static PlainSignInHandler BuildPlainSignInHandler()
        {
            ILogger<PlainSignInHandler> logger = Substitute.For<ILogger<PlainSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            return new PlainSignInHandler(logger, claimsProvider, BuildErrorTranslations());
        }

        private static PkceSignInHandler BuildPkceSignInHandler()
        {
            ILogger<PkceSignInHandler> logger = Substitute.For<ILogger<PkceSignInHandler>>();
            IUserClaimsProvider claimsProvider = Substitute.For<IUserClaimsProvider>();
            AuthCodeService authCodeProvider = BuildAuthCodeService();
            return new PkceSignInHandler(logger, claimsProvider, authCodeProvider, BuildErrorTranslations());
        }
    }
}
