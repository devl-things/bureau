namespace Bureau.Primitives.Features;

/// <summary>
/// Master feature switches. Bound from appsettings: "Features": { "Disabled": ["watson.nodes.analytics"] }
/// When a feature key is in Disabled, that feature returns 404 regardless of token scopes.
/// When absent from Disabled, the feature is enabled (opt-out model — default on).
/// </summary>
public sealed class FeaturesOptions
{
    public const string SectionName = "Features";

    /// <summary>Feature keys that are globally disabled on this host.</summary>
    public List<string> Disabled { get; set; } = [];

    public bool IsEnabled(string featureKey) => !Disabled.Contains(featureKey, StringComparer.OrdinalIgnoreCase);
}
