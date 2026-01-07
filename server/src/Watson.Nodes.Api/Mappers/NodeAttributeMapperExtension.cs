using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class NodeAttributeMapperExtension
    {
        public static NodeAttributeKey ToDomain(this AttributeKeyDto dto)
        {
            return new NodeAttributeKey
            {
                Key = dto.Key,
                Locale = dto.Locale
            };
        }
        public static NodeAttribute ToDomain(this AttributeDto dto)
        {
            return new NodeAttribute()
            {
                Key = dto.Key,
                Locale = dto.Locale,
                Type = dto.Type.ToDomain(),
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
