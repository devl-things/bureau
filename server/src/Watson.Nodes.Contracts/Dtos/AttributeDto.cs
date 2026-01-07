using System.Text.Json.Serialization;

namespace Watson.Nodes.Contracts.Dtos
{
    /// <summary>
    /// Represents a typed attribute value on a node.
    /// </summary>
    /// <remarks>
    /// Exactly one value field should be set based on <see cref="Type"/>:
    /// <list type="bullet">
    /// <item><description><c>String</c> → <see cref="ValueString"/></description></item>
    /// <item><description><c>Number</c> → <see cref="ValueNumber"/></description></item>
    /// <item><description><c>Bool</c> → <see cref="ValueBool"/></description></item>
    /// <item><description><c>Json</c> → <see cref="ValueJson"/></description></item>
    /// <item><description><c>Ref</c> → <see cref="RefNodeId"/></description></item>
    /// <item><description><c>Date</c> → <see cref="ValueDate"/></description></item>
    /// </list>
    /// </remarks>
    public sealed class AttributeDto
    {
        /// <summary>
        /// Attribute key name (recommended: lowercase snake_case, optionally namespaced).
        /// </summary>
        [JsonPropertyName("key")]
        public required string Key { get; init; }

        /// <summary>
        /// Optional locale for the attribute value, expressed as a BCP 47 language tag.
        /// </summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; init; }

        /// <summary>
        /// Declares which value field is populated for this attribute.
        /// </summary>
        [JsonPropertyName("type")]
        public required AttributeValueTypeContract Type { get; init; }

        /// <summary>
        /// String value (used when <see cref="Type"/> is <c>String</c>).
        /// </summary>
        [JsonPropertyName("valueString")]
        public string? ValueString { get; init; }

        /// <summary>
        /// Numeric value (used when <see cref="Type"/> is <c>Number</c>).
        /// </summary>
        [JsonPropertyName("valueNumber")]
        public decimal? ValueNumber { get; init; }

        /// <summary>
        /// Boolean value (used when <see cref="Type"/> is <c>Bool</c>).
        /// </summary>
        [JsonPropertyName("valueBool")]
        public bool? ValueBool { get; init; }

        /// <summary>
        /// JSON value serialized as a string (used when <see cref="Type"/> is <c>Json</c>).
        /// </summary>
        /// <remarks>
        /// The value is stored as raw JSON text. Consumers should treat it as JSON, not as an escaped string payload.
        /// </remarks>
        [JsonPropertyName("valueJson")]
        public string? ValueJson { get; init; }

        /// <summary>
        /// Reference to another node (used when <see cref="Type"/> is <c>Ref</c>).
        /// </summary>
        [JsonPropertyName("refNodeId")]
        public Guid? RefNodeId { get; init; }

        /// <summary>
        /// Date/time value (used when <see cref="Type"/> is <c>Date</c>).
        /// </summary>
        [JsonPropertyName("valueDate")]
        public DateTimeOffset? ValueDate { get; init; }
    }
}
