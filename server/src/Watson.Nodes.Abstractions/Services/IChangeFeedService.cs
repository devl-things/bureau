using Bureau;

namespace Watson.Nodes.Services
{
    //TODO missing implementation
    public interface IChangeFeedService
    {
        Task<CursorResult<ChangeEvent>> GetChangesAsync(ChangeFeedQuery request, CancellationToken cancellationToken = default);
    }
}
