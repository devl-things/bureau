using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sven.Tests.TestUtils;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sven.Tests.Fixtures
{
    public class SvenWebAppFactory : WebApplicationFactory<Program>
    {
        public TestLoggerProvider LoggerProvider { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Optional: clear default providers
                logging.AddProvider(LoggerProvider);
            });
            builder.ConfigureServices(services =>
                {
                    //ServiceDescriptor? s = services.FirstOrDefault(s => s.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>));
                    //while (s != null)
                    //{
                    //    services.Remove(s);
                    //    s = services.FirstOrDefault(s => s.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>));
                    //}


                    //// ✅ Replace "Cookies" scheme with our test handler
                    //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    //    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    //        CookieAuthenticationDefaults.AuthenticationScheme,
                    //        options => { });

                    // 👇 Optional: Make sure the default scheme is set to "Cookies"
                    //services.PostConfigure<AuthenticationOptions>(options =>
                    //{
                    //    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    //});
                });

            //builder.Configure(app =>
            //{
            //    // Add HTTPS redirection middleware
            //    app.UseHttpsRedirection();
            //});
        }
    }

    internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
