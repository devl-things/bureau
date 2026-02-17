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
            Result canonicalKey = CanonicalKeyConventions.Validate(command.CanonicalKey);
            if (canonicalKey.IsError)
            {
                return canonicalKey.Error;
            }
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

        protected virtual Result ValidateKeys(IReadOnlyList<string>? keys)
        {
            if (keys != null)
            {
                foreach (string key in keys)
                {
                    Result attributeKeyValidation = NodeAttributeConventions.ValidateKey(key);
                    if (attributeKeyValidation.IsError)
                    {
                        return attributeKeyValidation.Error;
                    }
                }
            }
            return true;
        }
        /// <inheritdoc />
        public Result ValidateSearch(SearchNodesQuery query)
        {
            Result scopeValidation = ScopeConventions.ValidateNotRequired(query.Filter.Scope);
            if (scopeValidation.IsError)
            {
                return scopeValidation.Error;
            }
            Result localeValidation = LocaleConventions.Validate(query.Filter.Locale);
            if (localeValidation.IsError)
            {
                return localeValidation.Error;
            }
            // Open to any number of keys, but this needs to be changed eventually
            Result queryKeysValidation = ValidateKeys(query.Filter.QueryAttributeKeys);
            if (queryKeysValidation.IsError)
            {
                return queryKeysValidation.Error;
            }
            Result projectionKeysValidation = ValidateKeys(query.Filter.AttributeKeys);
            if (projectionKeysValidation.IsError)
            {
                return projectionKeysValidation.Error;
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

        protected virtual Result ValidateAttributesByDefinition(CreateNodeCommand command, Dictionary<string, NodeAttributeDefinition> definitions, Dictionary<string, bool> required)
        {
            foreach (NodeAttribute attribute in command.Attributes)
            {
                Result attributeKeyValidation = ValidateAttributeKey(attribute);
                if (attributeKeyValidation.IsError)
                {
                    return attributeKeyValidation.Error;
                }
                Result attributeValueValidation = ValidateAttributeValue(attribute, definitions, required);
                if (attributeValueValidation.IsError)
                {
                    return attributeValueValidation.Error;
                }
            }
            if (required.Any(kvp => !kvp.Value))
            {
                string missingKeys = string.Join(", ", required.Where(kvp => !kvp.Value).Select(kvp => kvp.Key));
                return ResultError.From(ProblemCodes.Validation.Required, $"Missing required attributes: {missingKeys}");
            }
            return true;
        }

        private static Result ValidateAttributeValue(NodeAttribute attribute, Dictionary<string, NodeAttributeDefinition> definitions, Dictionary<string, bool> required)
        {
            if (definitions.TryGetValue(attribute.Key, out NodeAttributeDefinition? definition))
            {
                Result attributeResult = NodeAttributeConventions.ValidateAttributeByDefinition(attribute, definition);
                if (attributeResult.IsError)
                {
                    return attributeResult.Error;
                }
                if (required.ContainsKey(attribute.Key))
                {
                    required[attribute.Key] = true;
                }
            }
            else
            {
                Result attributeResult = NodeAttributeConventions.ValidateAttributeValue(attribute);
                if (attributeResult.IsError)
                {
                    return attributeResult.Error;
                }
            }

            return true;
        }
    }
}
