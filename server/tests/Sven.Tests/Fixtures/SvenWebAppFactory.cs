using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Data.TypeConfigurations;
using Sven.Tests.TestData;
using Sven.Tests.TestUtils;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Sven.Tests.Fixtures
{
    // Concrete SQLite in-memory SvenContext used only in tests.
    // SQLite is required (instead of EF InMemory) because repositories use
    // ExecuteDeleteAsync / ExecuteUpdateAsync which require a relational provider.
    internal sealed class SvenTestContext : SvenContext
    {
        public SvenTestContext(DbContextOptions<SvenTestContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // In-memory EF discovers SerializedData as a keyless entity via navigation scanning;
            // ignore it because it is stored as a JSON column (not a separate table).
            modelBuilder.Ignore<SerializedData>();
            // Apply the base entity configuration for ClientDb. The generic
            // ApplyConfigurationsFromAssembly cannot resolve open-generic configurations;
            // apply the closed concrete configuration directly instead.
            new ClientBaseTypeConfiguration<ClientDb>().Configure(modelBuilder.Entity<ClientDb>());
            new ClientFeatureBaseTypeConfiguration().Configure(modelBuilder.Entity<ClientFeatureDb>());
            new ClientFeatureExternalRequirementBaseTypeConfiguration().Configure(modelBuilder.Entity<ClientFeatureExternalRequirementDb>());
            new AuthCodeBaseTypeConfiguration().Configure(modelBuilder.Entity<AuthCodeDb>());
            new OAuthRequestBaseTypeConfiguration().Configure(modelBuilder.Entity<OAuthRequestDb>());
            new UserVerificationCodeBaseTypeConfiguration().Configure(modelBuilder.Entity<UserVerificationCodeDb>());
            new TicketBaseTypeConfiguration().Configure(modelBuilder.Entity<TicketDb>());
            new FailedExchangeAttemptBaseTypeConfiguration().Configure(modelBuilder.Entity<FailedExchangeAttemptDb>());
        }
    }

    public class SvenWebAppFactory : WebApplicationFactory<Program>
    {
        public TestLoggerProvider LoggerProvider { get; } = new();

        private readonly Dictionary<string, string?> _extraConfig = new();

        // Shared SQLite connection kept open for the lifetime of this factory instance.
        // All DI-resolved SvenTestContext instances share this connection so they all
        // operate on the same in-memory database and ExecuteDeleteAsync / ExecuteUpdateAsync work.
        private readonly SqliteConnection _sqliteConnection;

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
            ["Sven:InitialAccessToken"] = TestDataConstants.TestIat,
        };

        public SvenWebAppFactory()
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();
        }

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
                // Register a SQLite in-memory EF context so repositories can be resolved
                // without a real DB, while still supporting ExecuteDeleteAsync / ExecuteUpdateAsync.
                // All instances share the same SqliteConnection so they see the same database.
                SqliteConnection sharedConnection = _sqliteConnection;
                services.AddDbContext<SvenTestContext>(options =>
                    options.UseSqlite(sharedConnection));

                services.AddScoped<SvenContext>(sp =>
                    sp.GetRequiredService<SvenTestContext>());

                // Ensure the schema is created on first use.
                ServiceProvider sp2 = services.BuildServiceProvider();
                using (IServiceScope scope = sp2.CreateScope())
                {
                    SvenTestContext ctx = scope.ServiceProvider.GetRequiredService<SvenTestContext>();
                    ctx.Database.EnsureCreated();
                }
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(LoggerProvider);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _sqliteConnection.Dispose();
            }
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
