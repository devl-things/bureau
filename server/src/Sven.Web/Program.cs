using Bureau.AspNetCore.Cors;
using Bureau.AspNetCore.Logging;
using Bureau.AspNetCore.Middleware;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.V8;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Configurations.Providers;
using Sven.Data.SqlServer.Configurations;
using Sven.Web.Configurations;
using Sven.Middleware;
using Sven;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.SignIn;
using Sven.PageModels.Connect.SignUp;
using Sven.Pages.Connect;
using Sven.Services;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using WebOptimizer.Processors;

namespace Sven
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                // Skip ValidateOnBuild in Testing so unregistered DB context doesn't block startup
                options.ValidateOnBuild = !context.HostingEnvironment.IsEnvironment("Testing");
            });

            builder.Logging.AddBureauActivityTracking();

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddOptions<AuthOptions>().Bind(builder.Configuration.GetSection("Auth"))
                .Validate(options => options.AuthorizationCodeLifetime <= TimeSpan.FromMinutes(10))
                .ValidateOnStart();

            builder.Services.AddOptions<EncryptionKeysOptions>().Bind(builder.Configuration.GetSection("Encrypt"))
                .Validate(options => !string.IsNullOrWhiteSpace(options.SymKey) && options.SymKey.Length == 44)
                .ValidateOnStart();

            builder.Services.Configure<TokenVaultOptions>(builder.Configuration.GetSection("TokenVault"));

            builder.Services.AddSingleton<RsaSecurityKey>(provider =>
            {
                RSA rsa = RSA.Create(2048);
                return new RsaSecurityKey(rsa)
                {
                    KeyId = Guid.NewGuid().ToString()
                };
            });
            builder.Services.AddSingleton<ISymEncryptor, AesEncryptor>();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<DiscoveryService>();
            builder.Services.AddSingleton<IAuthCodeService, AuthCodeService>();
            builder.Services.AddScoped<INotificationService<UserVerificationCodeNotification>, EmailNotificationService<UserVerificationCodeNotification>>();
            builder.Services.AddScoped<INotificationService<PasswordResetNotification>, EmailNotificationService<PasswordResetNotification>>();
            builder.Services.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
            builder.Services.AddScoped<ICurrentUserProvider, UserClaimsProvider>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<ITokenProvider, SvenTokenProvider>();
            builder.Services.AddScoped<OAuthValidationFilter>();
            builder.Services.AddHttpClient();
            builder.Services.AddHostedService<TokenRefreshBackgroundService>();

            builder.Services.AddScoped<PlainSignInHandler>();
            builder.Services.AddScoped<PkceSignInHandler>();
            builder.Services.AddScoped<TicketSignInHandler>();
            builder.Services.AddScoped<ISignInHandlerFactory, SignInHandlerFactory>();
            builder.Services.AddScoped<SignUpFlowHelper>();
            builder.Services.AddScoped<PlainEnterEmailStepHandler>();
            builder.Services.AddScoped<PlainVerifyCodeStepHandler>();
            builder.Services.AddScoped<PlainSetPasswordStepHandler>();
            builder.Services.AddScoped<PlainFinalMessageStepHandler>();
            builder.Services.AddScoped<ForgotEnterEmailStepHandler>();
            builder.Services.AddScoped<ForgotCodeSentStepHandler>();
            builder.Services.AddScoped<ForgotSetPasswordStepHandler>();
            builder.Services.AddScoped<ForgotFinalMessageStepHandler>();
            builder.Services.AddScoped<PlainSignUpFlowDispatcher>();
            builder.Services.AddScoped<ForgotSignUpFlowDispatcher>();

            builder.Services.AddScoped<AccountTranslations>();
            builder.Services.AddScoped<ConnectTranslations>();
            builder.Services.AddScoped<ErrorTranslations>();

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddSvenSqlServer(builder.Configuration);
            }
            else
            {
                // In testing: skip SQL Server connection validation; test factory registers the DB context
                builder.Services.AddSvenCore();
            }

            // Rate limiting (SEC-02) — all endpoints partitioned by remote IP; client_id partitioning deferred
            builder.Services.AddSvenRateLimiter(builder.Configuration);

            builder.Services.AddBureauCors(builder.Configuration, builder.Environment);

            builder.Services.AddSvenAuthentication(builder.Configuration, providers =>
            {
                providers.AddGoogle();
                providers.AddMicrosoft();
            });
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute(Endpoints.Connect.SignUp, Endpoints.Connect.ForgotPassword);
            });

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();
                builder.Services.AddWebOptimizer(pipeline =>
                {
                    pipeline.AddScssBundle(StylesScriptNames.ConnectMin, "scss/connect.base.scss");
                    pipeline.AddScssBundle(StylesScriptNames.ConnectSignMin, "scss/connect.sign.scss");
                    pipeline.AddScssBundle(StylesScriptNames.SvenMin, "scss/sven.scss");

                    pipeline.AddJavaScriptBundle(JsScriptNames.SvenMin, new JsSettings() { GenerateSourceMap = true }, "js/bootstrap.bundle.min.js",
                        "js/layout.js");
                });
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.Services.MigrateSven();
            }

            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.UseWebOptimizer();
            }
            app.UseStaticFiles();

            app.UseRequestLocalization(options =>
            {
                var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("hr") };
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders =
                [
                    new CookieRequestCultureProvider(),
                    new QueryStringRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider(),
                ];
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseBureauCors();
            app.UseAuthentication();
            // Rate limiting: after UseAuthentication, before UseAuthorization — order is mandatory
            app.UseRateLimiter();
            app.UseAuthorization();
            app.UseMiddleware<CurrentUserMiddleware>();
            app.UseMiddleware<ApiExceptionHandlingMiddleware>();
            app.MapRazorPages();
            app.MapControllers();

            app.Run();
        }
    }
}
