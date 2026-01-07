using Bureau;

namespace Watson.Nodes.Services
{
    public interface IChangeFeedService
    {
        Task<CursorResult<ChangeEvent>> GetChangesAsync(ChangeFeedQuery request, CancellationToken cancellationToken = default);
    }
}
