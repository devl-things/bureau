using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class NodeAttributeMapperExtension
    {
        public static NodeAttribute ToDomain(this AttributeDto dto)
        {
            return new NodeAttribute(
                dto.Key,
                dto.Locale,
                dto.Type.ToDomain(),
                dto.ValueString,
                dto.ValueNumber,
                dto.ValueBool,
                dto.ValueJson,
                dto.RefNodeId,
                dto.ValueDate);
        }

        public static AttributeDto ToDto(this NodeAttribute attribute)
        {
            return new AttributeDto
            {
                Key = attribute.Key,
                Locale = attribute.Locale,
                Type = attribute.Type.ToContract(),
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
