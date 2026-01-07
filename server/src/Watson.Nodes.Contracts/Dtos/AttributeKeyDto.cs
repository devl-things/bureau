using System.Text.Json.Serialization;

namespace Watson.Nodes.Contracts.Dtos
{
    /// <summary>
    /// Identifies a single attribute entry on a node.
    /// </summary>
    /// <remarks>
    /// Uniqueness is defined by <c>(NodeId, Key, Locale)</c>. The same <see cref="Key"/> may exist multiple times
    /// on a node for different locales.
    /// </remarks>
    public sealed class AttributeKeyDto
    {
        /// <summary>
        /// Attribute key name (recommended: lowercase snake_case, optionally namespaced, e.g. <c>catalog:ean</c>).
        /// </summary>
        /// <remarks>
        /// Examples: <c>label</c>, <c>description</c>, <c>fat_percent</c>, <c>catalog:ean</c>.
        /// </remarks>
        [JsonPropertyName("key")]
        public required string Key { get; init; }

        /// <summary>
        /// Optional locale for the attribute value, expressed as a BCP 47 language tag.
        /// </summary>
        /// <remarks>
        /// Examples: <c>en</c>, <c>en-US</c>, <c>hr</c>, <c>hr-HR</c>. Null means invariant / non-localized value.
        /// </remarks>
        [JsonPropertyName("locale")]
        public string? Locale { get; init; }
    }
}
