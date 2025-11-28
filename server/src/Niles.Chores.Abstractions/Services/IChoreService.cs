using Niles.Chores;
using Niles.Chores.Abstractions.Models;

namespace Niles.Chores.Abstractions.Services
{
    public interface IChoreService
    {
        Task<bool> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Chore>> ListChoresAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<Chore>> ListChoresPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<bool> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<bool> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
    }
}
