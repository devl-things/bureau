using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.V8;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Data.SqlServer.Configurations;
using Sven.Middleware;
using Sven.Models;
using Sven.PageModels;
using Sven.PageModels.Account;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.SignIn;
using Sven.PageModels.Connect.SignUp;
using Sven.Pages.Connect;
using Sven.Services;
using System.Globalization;
using System.Security.Cryptography;
using WebOptimizer.Processors;

namespace Sven
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // TODO check this
            builder.WebHost.UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
            builder.Services.AddOptions<GoogleOptions>().Bind(builder.Configuration.GetSection("Google"))
                .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.ClientId) &&
                    !string.IsNullOrWhiteSpace(options.ClientSecret),
                    "Google configuration is missing or invalid.")
                .ValidateOnStart();

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddOptions<AuthOptions>().Bind(builder.Configuration.GetSection("Auth"))
                .Validate(options => options.AuthorizationCodeLifetime <= TimeSpan.FromMinutes(10))
                .ValidateOnStart();
            builder.Services.AddOptions<EncryptionKeysOptions>().Bind(builder.Configuration.GetSection("Encrypt"))
                .Validate(options => !string.IsNullOrWhiteSpace(options.SymKey) && options.SymKey.Length == 44)
                .ValidateOnStart();

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
            builder.Services.AddSingleton<IStore<string, AuthCode>, InMemoryStore<string, AuthCode>>();
            builder.Services.AddSingleton<IStore<string, OAuthRequest>, InMemoryStore<string, OAuthRequest>>();
            builder.Services.AddSingleton<IStore<string, RefreshToken>, InMemoryStore<string, RefreshToken>>();
            builder.Services.AddSingleton<IStore<string, string>, InMemoryStore<string, string>>();
            builder.Services.AddSingleton<IStore<string, UserVerificationCode>, InMemoryStore<string, UserVerificationCode>>();
            builder.Services.AddSingleton<AuthCodeProvider>();
            builder.Services.AddScoped<INotificationService<UserVerificationCodeNotification>, EmailNotificationService<UserVerificationCodeNotification>>();
            builder.Services.AddScoped<INotificationService<PasswordResetNotification>, EmailNotificationService<PasswordResetNotification>>();
            builder.Services.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
            builder.Services.AddScoped<ICurrentUserProvider, UserClaimsProvider>();
            builder.Services.AddScoped<IUserProvider, UserProvider>();
            builder.Services.AddScoped<IClientProvider, ClientProvider>();
            builder.Services.AddScoped<ITokenProvider, SvenTokenProvider>();
            builder.Services.AddScoped<OAuthValidationFilter>();

            builder.Services.AddScoped<IPageModelFactory<PageContext, SignInPageModel>, SignInPageModelFactory>();
            builder.Services.AddScoped<PkceSignInPageModel>();
            builder.Services.AddScoped<PlainSignInPageModel>();
            builder.Services.AddScoped<TicketSignInPageModel>();
            builder.Services.AddScoped<IPageModelFactory<SignUpModel, SignUpPageModel>, SignUpPageModelFactory>();
            builder.Services.AddScoped<PlainSignUpPageModel>();
            builder.Services.AddScoped<ForgotSignUpPageModel>();

            builder.Services.AddScoped<AccountTranslations>();
            builder.Services.AddScoped<ConnectTranslations>();

            builder.Services.AddSvenSqlServer(options =>
            {
                options.ConnectionString = builder.Configuration.GetConnectionString("SqlServer");
            });


            // Config for CORS (allow React dev server access)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Cookie + external providers
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = Endpoints.Connect.SignIn;
                options.ReturnUrlParameter = AuthConstants.PropertyNames.RedirectUrl;
            })
            .AddCookie(AuthConstants.AuthenticationSchemes.External)
            .AddGoogle(AuthConstants.ExternalSchemes.Google, options =>
            {
                builder.Configuration.Bind("Google", options);
                options.SignInScheme = AuthConstants.AuthenticationSchemes.External;
            });
            // TODO when I get ClientId and ClientSecret
            //.AddMicrosoftAccount(options =>
            //{
            //    options.ClientId = builder.Configuration["Microsoft:ClientId"];
            //    options.ClientSecret = builder.Configuration["Microsoft:ClientSecret"];
            //    options.CallbackPath = "/signin-microsoft";
            //});

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute(Endpoints.Connect.SignUp, Endpoints.Connect.ForgotPassword);
            });

            builder.Services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();
            builder.Services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddScssBundle(StylesScriptNames.ConnectMin, "scss/connect.base.scss");
                pipeline.AddScssBundle(StylesScriptNames.ConnectSignMin, "scss/connect.sign.scss");
                pipeline.AddScssBundle(StylesScriptNames.SvenMin, "scss/sven.scss");

                pipeline.AddJavaScriptBundle(JsScriptNames.SvenMin, new JsSettings() { GenerateSourceMap = true }, "js/bootstrap.bundle.min.js",
                    "js/layout.js");
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            app.UseWebOptimizer();
            app.UseStaticFiles(); // if you want CSS

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
            app.MapRazorPages();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

#if DEBUG
            //app.Use(async (context, next) =>
            //{
            //    if (!context.User.Identity?.IsAuthenticated ?? true)
            //    {
            //        var claims = new List<Claim>
            //        {
            //            new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
            //            new Claim(ClaimTypes.Name, "debug@example.com"),
            //            new Claim(ClaimTypes.Email, "debug@example.com")
            //        };

            //        var identity = new ClaimsIdentity(claims, "Debug");
            //        var principal = new ClaimsPrincipal(identity);

            //        await context.SignInAsync(principal);
            //    }

            //    await next();
            //});
#endif
            app.UseMiddleware<CurrentUserMiddleware>();
            app.MapControllers();



            app.Run();
        }
    }
}
