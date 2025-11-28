using Niles.Chores;

namespace Niles.Chores.Abstractions.Services
{
    public interface IChoreService
    {
        Task<bool> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Chore>> ListChoresAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<bool> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
    }
}
