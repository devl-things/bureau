using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sven.Tests.Security
{
    [Trait("Category", "Phase1")]
    public class AesEncryptorTests
    {
        private static readonly Dictionary<string, string?> BaseTestConfig = new()
        {
            ["Google:ClientId"] = "test-google-client-id",
            ["Google:ClientSecret"] = "test-google-client-secret",
            ["Microsoft:ClientId"] = "test-microsoft-client-id",
            ["Microsoft:ClientSecret"] = "test-microsoft-client-secret"
        };

        [Fact]
        public void Application_MissingEncryptSymKey_ThrowsOnStartup()
        {
            // A WebApplicationFactory configured without Encrypt:SymKey causes startup to fail.
            // The EncryptionKeysOptions.ValidateOnStart() guard throws OptionsValidationException
            // (which wraps the validation failure).
            Dictionary<string, string?> config = new(BaseTestConfig)
            {
                ["Encrypt:SymKey"] = null
            };

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((_, cfg) =>
                    {
                        cfg.AddInMemoryCollection(config);
                    });
                });

            Exception ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            // The exception chain must include a startup failure. The root cause is either an
            // InvalidOperationException (from EncryptionKeyStartupFilter) or OptionsValidationException
            // (from ValidateOnStart). Either proves the guard is in place.
            bool guardFired = ContainsGuardException(ex);
            Assert.True(guardFired,
                $"Expected startup to fail when Encrypt:SymKey is missing. Actual: {ex.GetType().Name}: {ex.Message}");
        }

        [Fact]
        public void Application_ValidEncryptSymKey_StartsNormally()
        {
            // A factory with a valid Encrypt:SymKey present starts without exception.
            // 32 bytes = 256 bits, Base64-encoded (44-character string).
            string validKey = Convert.ToBase64String(new byte[32]);
            Dictionary<string, string?> config = new(BaseTestConfig)
            {
                ["Encrypt:SymKey"] = validKey
            };

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((_, cfg) =>
                    {
                        cfg.AddInMemoryCollection(config);
                    });
                });

            HttpClient client = factory.CreateClient();
            Assert.NotNull(client);
        }

        private static bool ContainsGuardException(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is InvalidOperationException)
                {
                    return true;
                }

                if (ex is OptionsValidationException)
                {
                    return true;
                }

                if (ex is AggregateException aggEx)
                {
                    foreach (Exception inner in aggEx.InnerExceptions)
                    {
                        if (ContainsGuardException(inner))
                        {
                            return true;
                        }
                    }
                }

                ex = ex.InnerException;
            }
            return false;
        }
    }
}
