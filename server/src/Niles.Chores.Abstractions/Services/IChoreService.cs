using Bureau;

namespace Niles.Chores.Services
{
    public interface IChoreService
    {
        Task<Result<Chore>> GetChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Chore>> GetChoresAsync(SearchParameters searchParameters, CancellationToken cancellationToken = default);

        Task<Result<Chore>> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
    }
}
