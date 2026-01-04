using Bureau;

namespace Watson.Nodes.Abstractions.Services
{
    public interface IChangeFeedService
    {
        Task<CursorResult<ChangeEvent>> GetChangesAsync(long after, int limit, ChangeMode mode, CancellationToken cancellationToken = default);
    }
}
