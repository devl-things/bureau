using Niles.Chores;

namespace Niles.Chores.Abstractions.Services
{
    public interface IChoreService
    {
        Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Chore>> ListChoresAsync(CancellationToken cancellationToken = default);
        Task<Chore> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<Chore?> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default);
        Task<bool> DeleteChoreAsync(int id, CancellationToken cancellationToken = default);
    }
}
