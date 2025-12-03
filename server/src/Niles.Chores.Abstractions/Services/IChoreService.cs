using Bureau;

namespace Niles.Chores.Services
{
    public interface IChoreService
    {
        Task<Result<Chore>> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Chore>> ListChoresPagedAsync(SearchRequest pagination, CancellationToken cancellationToken = default);
        Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Chore>> ListChoresPagedWithCriticalAsync(SearchRequest pagination, CancellationToken cancellationToken = default);
    }
}
