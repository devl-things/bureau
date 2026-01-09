using Bureau;
using Bureau.Primitives.Errors;
using System.Text.RegularExpressions;

namespace Watson.Nodes.Conventions
{
    /// <summary>
    /// Defines canonical conventions for node attribute keys and provides validation helpers.
    /// </summary>
    /// <remarks>
    /// Attribute keys identify metadata attached to nodes.
    ///
    /// Normalization and validation are intentionally separated:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Normalization is typically performed at API or mapping boundaries (e.g. DTO → domain).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Validation is performed in the service layer to enforce canonical rules.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Canonical key rules:
    /// <list type="bullet">
    /// <item><description>lowercase</description></item>
    /// <item><description>snake_case-style tokens</description></item>
    /// <item><description>allowed characters: <c>a-z</c>, <c>0-9</c>, <c>_</c>, <c>:</c></description></item>
    /// <item><description>length: 1–64 characters</description></item>
    /// </list>
    /// </remarks>
    public static class NodeAttributeConventions
    {
        private const int MaxKeyLength = 64;

        // lowercase snake_case with optional namespace (e.g. catalog:ean)
        private static readonly Regex AttributeKeyRegex =
            new Regex("^[a-z0-9:_]{1,64}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        private static readonly Dictionary<AttributeValueType, Func<NodeAttribute, object?>> _valueAccessors = new()
            {
                { AttributeValueType.String, x => x.ValueString },
                { AttributeValueType.Number, x => x.ValueNumber },
                { AttributeValueType.Bool, x => x.ValueBool },
                { AttributeValueType.Json, x => x.ValueJson },
                { AttributeValueType.Ref, x => x.RefNodeId },
                { AttributeValueType.Date, x => x.ValueDate },
            };

        /// <summary>
        /// Normalizes an attribute key to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization performs minimal, safe transformations only:
        /// <list type="bullet">
        /// <item><description>trims leading and trailing whitespace</description></item>
        /// <item><description>converts to lowercase</description></item>
        /// </list>
        /// This method does not perform validation and may return values that do not conform
        /// to the canonical convention. Validation must be performed separately.
        /// </remarks>
        /// <param name="key">Raw attribute key input.</param>
        /// <returns>
        /// The normalized attribute key, or <c>null</c> if the input is <c>null</c>
        /// or consists only of whitespace.
        /// </returns>
        public static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null!;
            }

            return key.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Validates whether an attribute key conforms to the canonical convention.
        /// </summary>
        /// <remarks>
        /// This method assumes the input has already been normalized (trimmed and lowercased).
        /// A <c>null</c> value is invalid for attribute keys.
        /// </remarks>
        /// <param name="key">Attribute key to validate (expected normalized).</param>
        /// <returns>
        /// A successful <see cref="Result"/> if the key is valid; otherwise an error result
        /// describing the validation failure.
        /// </returns>
        public static Result ValidateKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return ResultError.From(ProblemCodes.Validation.Required, "Attribute key is required.");
            }

            if (key.Length > MaxKeyLength)
            {
                return ResultError.From(ProblemCodes.Validation.TooLong, $"Invalid attribute key length. Keys must be 1–{MaxKeyLength} characters long.");
            }

            if (!AttributeKeyRegex.IsMatch(key))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, $"Invalid attribute key. Keys must be lowercase, 1–{MaxKeyLength} characters long, and contain only a–z, 0–9, '_', ':' characters.");
            }

            return true;
        }

        /// <summary>
        /// Validates that an attribute value matches its declared <see cref="AttributeValueType"/>
        /// and that no more than one value field is populated.
        /// </summary>
        /// <remarks>
        /// This validation enforces structural consistency between the declared
        /// <see cref="NodeAttribute.Type"/> and the value fields on the attribute.
        ///
        /// Rules:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Zero value fields populated is valid and represents a nullable or unset attribute.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If a value field is populated, it MUST correspond to the field implied by
        /// <see cref="NodeAttribute.Type"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// No more than one value field may be populated at any time.
        /// </description>
        /// </item>
        /// </list>
        ///
        /// This method validates value shape only. It does not enforce semantic rules such as
        /// requiredness, value ranges, or business meaning.
        /// </remarks>
        /// <param name="attribute">
        /// Attribute to validate. The attribute key and locale are expected to be normalized
        /// prior to calling this method.
        /// </param>
        /// <returns>
        /// A successful <see cref="Result"/> if the value shape is valid; otherwise an error
        /// result describing the validation failure.
        /// </returns>
        public static Result ValidateAttributeValue(NodeAttribute attribute)
        {
            if (attribute == null)
            {
                return ResultError.From(ProblemCodes.Validation.Required, "Attribute is required.");
            }
            if (!_valueAccessors.ContainsKey(attribute.Type))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, $"Invalid attribute type '{attribute.Type}'.");
            }

            int populated = 0;
            object? wantedValue = null;
            object? writtenValue = null;
            AttributeValueType? writtenType = null;

            foreach (KeyValuePair<AttributeValueType, Func<NodeAttribute, object?>> pair in _valueAccessors)
            {
                AttributeValueType type = pair.Key;
                Func<NodeAttribute, object?> accessor = pair.Value;

                object? value = accessor(attribute);

                if (type == attribute.Type)
                {
                    wantedValue = value;
                }

                if (value != null)
                {
                    populated++;
                    writtenValue = value;
                    writtenType = type;
                }
            }

            if (populated > 1)
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, "Invalid attribute value. Only one value field may be set.");
            }

            if (populated == 0 || wantedValue != null)
            {
                return true;
            }

            return ResultError.From(ProblemCodes.Validation.InvalidFormat, $"Attribute value does not match declared type '{attribute.Type}'.",
                $"Attribute type is '{attribute.Type}', but value was provided via '{writtenType}' with value '{writtenValue}'.");
        }
    }
}
