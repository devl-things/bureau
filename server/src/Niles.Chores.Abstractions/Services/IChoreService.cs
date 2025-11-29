using Niles.Chores;
using Niles.Chores.Abstractions.Models;

namespace Niles.Chores.Abstractions.Services
{
    public interface IChoreService
    {
        Task<Result<Chore>> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default); 
        Task<PagedResult<Chore>> ListChoresPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Result<Chore>> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Result> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Chore>> ListChoresPagedWithCriticalAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<(Chore Chore, bool IsCritical)?> GetChoreWithCriticalAsync(int id, CancellationToken cancellationToken = default);
    }
}
