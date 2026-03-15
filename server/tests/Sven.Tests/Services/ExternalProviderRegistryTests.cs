using Sven.Configurations.Providers;
using Sven.Services;
using Sven;
using Sven.Services;

namespace Sven.Tests.Services
{
    public class ExternalProviderRegistryTests
    {
        private static ExternalProviderRegistry BuildRegistry(
            IEnumerable<RegisteredExternalProvider> providers,
            Dictionary<string, List<ExternalScope>>? scopes = null)
        {
            return new ExternalProviderRegistry(providers, scopes ?? new());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Providers_ReturnsAllRegistered()
        {
            var google = new RegisteredExternalProvider("google", "Google", "bi-google", "btn-google", true);
            var microsoft = new RegisteredExternalProvider("microsoft", "Microsoft", "bi-microsoft", "btn-microsoft", true);

            IExternalProviderRegistry registry = BuildRegistry([google, microsoft]);

            Assert.Equal(2, registry.Providers.Count);
            Assert.Contains(registry.Providers, p => p.ProviderKey == "google");
            Assert.Contains(registry.Providers, p => p.ProviderKey == "microsoft");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetScopes_KnownProvider_ReturnsDeclaredScopes()
        {
            var provider = new RegisteredExternalProvider("google", "Google", "bi-google", "btn-google", true);
            var scopes = new Dictionary<string, List<ExternalScope>>
            {
                ["google"] =
                [
                    GoogleScopes.GmailReadonly,
                    GoogleScopes.CalendarReadonly
                ]
            };

            IExternalProviderRegistry registry = BuildRegistry([provider], scopes);

            IReadOnlyList<ExternalScope> result = registry.GetScopes("google");

            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.BureauKey == "google.gmail.readonly");
            Assert.Contains(result, s => s.BureauKey == "google.calendar.readonly");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetScopes_UnknownProvider_ReturnsEmpty()
        {
            IExternalProviderRegistry registry = BuildRegistry([]);

            IReadOnlyList<ExternalScope> result = registry.GetScopes("unknown");

            Assert.Empty(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetScopes_ProviderWithNoScopes_ReturnsEmpty()
        {
            var provider = new RegisteredExternalProvider("google", "Google", "bi-google", "btn-google", true);

            IExternalProviderRegistry registry = BuildRegistry([provider]);

            IReadOnlyList<ExternalScope> result = registry.GetScopes("google");

            Assert.Empty(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Providers_EmptyRegistry_ReturnsEmpty()
        {
            IExternalProviderRegistry registry = BuildRegistry([]);

            Assert.Empty(registry.Providers);
        }
    }
}
