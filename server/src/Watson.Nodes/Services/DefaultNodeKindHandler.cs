using Bureau;
using Bureau.Primitives.Errors;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Constants;
using Watson.Nodes.Conventions;

namespace Watson.Nodes.Services
{
    /// <summary>
    /// Default handler providing permissive validation and baseline defaults.
    /// </summary>
    public class DefaultNodeKindHandler : INodeKindHandler
    {
        protected static readonly List<string> DefaultSearchKeys =
            new List<string>
            {
                NodeAttributeKeys.Label
            };

        protected static readonly List<string> DefaultSummaryKeys =
            new List<string>
            {
                NodeAttributeKeys.Label,
                NodeAttributeKeys.Description
            };
        /// <inheritdoc />
        public virtual NodeKind Kind
        {
            get { return NodeKind.None; }
        }
        /// <inheritdoc />
        public virtual Result ValidateCreate(CreateNodeCommand command)
        {
            Result scopeValidation = ScopeConventions.Validate(command.Scope);
            if (scopeValidation.IsError)
            {
                return scopeValidation.Error;
            }
            Result attributesValidation = ValidateAttributes(command.Attributes);
            if (attributesValidation.IsError)
            {
                return attributesValidation.Error;
            }
            return true;
        }
        /// <inheritdoc />
        public virtual Result ValidatePatch(Guid nodeId, PatchNodeAttributesCommand command)
        {
            bool hasChange = false;
            if (command.Set != null && command.Set.Any())
            {
                Result setAttributesValidation = ValidateAttributes(command.Set);
                if (setAttributesValidation.IsError)
                {
                    return setAttributesValidation.Error;
                }
                hasChange = true;
            }
            if (command.Remove != null && command.Remove.Any())
            {
                foreach (NodeAttributeKey attributeKey in command.Remove)
                {
                    Result attributeKeyValidation = ValidateAttributeKey(attributeKey);
                    if (attributeKeyValidation.IsError)
                    {
                        return attributeKeyValidation.Error;
                    }
                }
                hasChange = true;
            }
            if (!hasChange)
            {
                return ResultError.From(ProblemCodes.Validation.Required, "At least one attribute must be set or removed", "No attributes were provided to set or remove.");
            }
            return true;
        }

        protected virtual Result ValidateAttributes(IEnumerable<NodeAttribute> attributes)
        {
            foreach (NodeAttribute attribute in attributes)
            {
                Result attributeKeyValidation = ValidateAttributeKey(attribute);
                if (attributeKeyValidation.IsError)
                {
                    return attributeKeyValidation.Error;
                }
                Result attributeValidation = NodeAttributeConventions.ValidateAttributeValue(attribute);
                if (attributeValidation.IsError)
                {
                    return attributeValidation.Error;
                }
            }
            return true;
        }

        protected virtual Result ValidateAttributeKey(NodeAttributeKey attributeKey)
        {
            Result keyValidation = NodeAttributeConventions.ValidateKey(attributeKey.Key);
            if (keyValidation.IsError)
            {
                return keyValidation.Error;
            }
            Result localeValidation = LocaleConventions.Validate(attributeKey.Locale);
            if (localeValidation.IsError)
            {
                return localeValidation.Error;
            }
            return true;
        }
        /// <inheritdoc />
        public virtual Task<Result> AfterCreateAsync(Node node, CreateNodeCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Result());
        }
        /// <inheritdoc />
        public virtual List<string> GetDefaultSearchAttributeKeys()
        {
            return DefaultSearchKeys;
        }
        /// <inheritdoc />
        public virtual List<string> GetDefaultSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }
        /// <inheritdoc />
        public List<string> GetSearchAttributeKeys(IReadOnlyList<string>? attributeKeys)
        {
            if (attributeKeys != null && attributeKeys.Count > 0)
            {
                return [.. attributeKeys.Distinct()];
            }

            return GetDefaultSearchAttributeKeys();
        }
        /// <inheritdoc />
        public List<string> GetProjectionAttributeKeys(IReadOnlyList<string>? attributeKeys)
        {
            if (attributeKeys != null && attributeKeys.Count > 0)
            {
                return [.. attributeKeys.Distinct()];
            }

            return GetDefaultSummaryAttributeKeys();
        }

        protected static Result RequireCanonicalKey(CreateNodeCommand command, string nodeKindName)
        {
            if (string.IsNullOrWhiteSpace(command.CanonicalKey))
            {
                return ResultError.From(ProblemCodes.Validation.Required, $"{nameof(command.CanonicalKey)} is required", $"{nameof(command.CanonicalKey)} is required for " + nodeKindName + " nodes.");
            }

            return true;
        }
    }
}
