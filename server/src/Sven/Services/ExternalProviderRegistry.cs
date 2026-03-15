using Sven;
using Sven.Services;

namespace Sven.Services
{
    internal sealed class ExternalProviderRegistry : IExternalProviderRegistry
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<ExternalScope>> _scopes;

        public IReadOnlyList<RegisteredExternalProvider> Providers { get; }

        internal ExternalProviderRegistry(
            IEnumerable<RegisteredExternalProvider> providers,
            IDictionary<string, List<ExternalScope>> scopes)
        {
            Providers = providers.ToList().AsReadOnly();
            _scopes = scopes.ToDictionary(
                k => k.Key,
                v => (IReadOnlyList<ExternalScope>)v.Value.AsReadOnly());
        }

        public IReadOnlyList<ExternalScope> GetScopes(string providerKey)
            => _scopes.TryGetValue(providerKey, out IReadOnlyList<ExternalScope>? scopes)
                ? scopes
                : Array.Empty<ExternalScope>();
    }
}
