using Watson.Nodes.Models;

namespace Watson.Nodes.Mappers
{
    internal static class NodeEdgeMapperExtension
    {
        public static NodeEdgeDb ToDb(this NodeEdge edge, Guid sourceNodeId)
        {
            return new NodeEdgeDb
            {
                SourceNodeId = sourceNodeId,
                TargetNodeId = edge.TargetNodeId,
                Purpose = edge.Purpose,
                OrderIndex = edge.OrderIndex
            };
        }

        public static NodeEdge ToDomain(this NodeEdgeDb db)
        {
            return new NodeEdge
            {
                SourceNodeId = db.SourceNodeId,
                TargetNodeId = db.TargetNodeId,
                Purpose = db.Purpose,
                OrderIndex = db.OrderIndex
            };
        }
    }
}
