using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class EdgeMapperExtension
    {
        public static CreateEdgeCommand ToDomain(this CreateEdgeRequest request)
        {
            return new CreateEdgeCommand
            {
                TargetNodeId = request.TargetNodeId,
                Purpose = EdgeConventions.Normalize(request.Purpose),
                OrderIndex = request.OrderIndex
            };
        }

        public static RemoveEdgeCommand ToDomain(this RemoveEdgeRequest request)
        {
            return new RemoveEdgeCommand
            {
                TargetNodeId = request.TargetNodeId,
                Purpose = EdgeConventions.Normalize(request.Purpose)
            };
        }

        public static EdgeDto ToDto(this NodeEdge edge)
        {
            return new EdgeDto
            {
                TargetNodeId = edge.TargetNodeId,
                Purpose = edge.Purpose,
                OrderIndex = edge.OrderIndex
            };
        }
    }
}
