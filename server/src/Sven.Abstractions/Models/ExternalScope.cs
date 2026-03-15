namespace Sven
{
    /// <summary>
    /// Declares an OAuth scope that an external provider exposes for bureau apps to consume via token exchange.
    /// Bindable from configuration (ExternalProviders:{provider}:Scopes section).
    /// </summary>
    public record ExternalScope
    {
        /// <summary>Bureau-scoped key used in JWT claims and bureau_features registration, e.g. "google.gmail.readonly".</summary>
        public string BureauKey { get; init; } = string.Empty;

        /// <summary>The actual OAuth scope string sent to the provider, e.g. "https://www.googleapis.com/auth/gmail.readonly".</summary>
        public string ProviderScope { get; init; } = string.Empty;

        /// <summary>Human-readable description shown in consent UI.</summary>
        public string Description { get; init; } = string.Empty;
    }
}
