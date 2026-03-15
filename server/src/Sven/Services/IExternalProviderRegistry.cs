using Sven;

namespace Sven.Services
{
    internal interface IExternalProviderRegistry
    {
        /// <summary>All providers registered at startup.</summary>
        IReadOnlyList<RegisteredExternalProvider> Providers { get; }

        /// <summary>Returns the bureau-declared scopes for a provider, or empty list if provider not found.</summary>
        IReadOnlyList<ExternalScope> GetScopes(string providerKey);
    }
}
