using Bureau;

namespace Watson.Nodes.Services
{
    public interface INodeEdgeService
    {
        Task<Result<NodeEdge>> AddEdgeAsync(Guid sourceNodeId, CreateEdgeCommand command, CancellationToken cancellationToken = default);
        Task<Result> RemoveEdgeAsync(Guid sourceNodeId, RemoveEdgeCommand command, CancellationToken cancellationToken = default);
        Task<CursorResult<NodeEdge>> GetEdgesAsync(Guid sourceNodeId, SearchEdgesQuery query, CancellationToken cancellationToken = default);
    }
}
