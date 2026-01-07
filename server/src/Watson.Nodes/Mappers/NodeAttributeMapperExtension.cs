using Watson.Nodes.Models;

namespace Watson.Nodes.Mappers
{
    internal static class NodeAttributeMapperExtension
    {
        public static NodeAttributeDb ToDb(this NodeAttribute attribute, Guid nodeId)
        {
            return new NodeAttributeDb
            {
                NodeId = nodeId,
                Key = attribute.Key,
                Locale = attribute.Locale,
                Type = attribute.Type,
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
