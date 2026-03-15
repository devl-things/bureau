using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sven.Data.Contexts;
using Sven.Tests.TestUtils;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sven.Tests.Fixtures
{
    // Concrete in-memory SvenContext used only in tests.
    internal sealed class SvenTestContext : SvenContext
    {
        public SvenTestContext(DbContextOptions<SvenTestContext> options) : base(options)
        {
        }
    }

    public class SvenWebAppFactory : WebApplicationFactory<Program>
    {
        public TestLoggerProvider LoggerProvider { get; } = new();

        private readonly Dictionary<string, string?> _extraConfig = new();

        // Minimal valid configuration required to pass ValidateOnStart checks.
        // SymKey must be a base-64 string of exactly 44 characters (32-byte AES key).
        // Google/Microsoft ClientId and ClientSecret must be non-empty to pass validation.
        private static readonly Dictionary<string, string?> BaseTestConfig = new()
        {
            ["Encrypt:SymKey"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            ["Auth:AuthorizationCodeLifetime"] = "00:05:00",
            ["Jwt:Issuer"] = "https://test.localhost",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:IdTokenLifetime"] = "00:05:00",
            ["Jwt:RefreshTokenLifetime"] = "30.00:00:00",
            ["Jwt:AccessTokenLifetime"] = "01:00:00",
            ["ConnectionStrings:SvenDb"] = null,
            ["Google:ClientId"] = "test-google-client-id",
            ["Google:ClientSecret"] = "test-google-client-secret",
            ["Microsoft:ClientId"] = "test-microsoft-client-id",
            ["Microsoft:ClientSecret"] = "test-microsoft-client-secret",
        };

        public SvenWebAppFactory WithExtraConfig(Dictionary<string, string?> extraConfig)
        {
            SvenWebAppFactory derived = new SvenWebAppFactory();
            foreach (KeyValuePair<string, string?> entry in _extraConfig)
            {
                derived._extraConfig[entry.Key] = entry.Value;
            }
            foreach (KeyValuePair<string, string?> entry in extraConfig)
            {
                derived._extraConfig[entry.Key] = entry.Value;
            }
            return derived;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(BaseTestConfig);
                if (_extraConfig.Count > 0)
                {
                    config.AddInMemoryCollection(_extraConfig);
                }
            });

            builder.ConfigureServices(services =>
            {
                // Register an in-memory EF context so repositories can be resolved without a real DB.
                services.AddDbContext<SvenTestContext>(options =>
                    options.UseInMemoryDatabase("SvenTestDb"));

                services.AddScoped<SvenContext>(sp =>
                    sp.GetRequiredService<SvenTestContext>());
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(LoggerProvider);
            });
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
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
