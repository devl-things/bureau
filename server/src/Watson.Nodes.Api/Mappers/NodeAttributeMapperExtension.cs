using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Contracts.Dtos;
using Watson.Nodes.Conventions;

namespace Watson.Nodes.Api.Mappers
{
    public static class NodeAttributeMapperExtension
    {
        public static NodeAttributeKey ToDomain(this AttributeKeyDto dto)
        {
            return new NodeAttributeKey(dto.Key)
            {
                Locale = dto.Locale
            };
        }
        public static NodeAttribute ToDomain(this AttributeDto dto)
        {
            return new NodeAttribute(NodeAttributeConventions.NormalizeKey(dto.Key))
            {
                Locale = LocaleConventions.Normalize(dto.Locale),
                Type = dto.Type.ToDomainValueType(),
                ValueString = dto.ValueString,
                ValueNumber = dto.ValueNumber,
                ValueBool = dto.ValueBool,
                ValueJson = dto.ValueJson,
                RefNodeId = dto.RefNodeId,
                ValueDate = dto.ValueDate
            };
        }

        public static AttributeDto ToDto(this NodeAttribute attribute)
        {
            return new AttributeDto
            {
                Key = attribute.Key,
                Locale = attribute.Locale,
                Type = attribute.Type.ToContractValueType(),
                ValueString = attribute.ValueString,
                ValueNumber = attribute.ValueNumber,
                ValueBool = attribute.ValueBool,
                ValueJson = attribute.ValueJson,
                RefNodeId = attribute.RefNodeId,
                ValueDate = attribute.ValueDate
            };
        }
    }
}
