using Bureau;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Definitions;

namespace Watson.Nodes.Services
{
    public sealed class VariantNodeKindHandler : DefaultNodeKindHandler
    {
        public override NodeKind Kind
        {
            get { return NodeKind.Variant; }
        }

        public override Result ValidateCreate(CreateNodeCommand command)
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
            return ValidateAttributesByDefinition(command, VariantDefinition.AttributeDefinitions, VariantDefinition.GetRequiredTemplate());
        }

        public override List<string> GetDefaultSearchAttributeKeys()
        {
            return VariantDefinition.VariantSearchKeys;
        }

        override public List<string> GetDefaultSummaryAttributeKeys()
        {
            return VariantDefinition.VariantSummaryKeys;
        }
    }
}
