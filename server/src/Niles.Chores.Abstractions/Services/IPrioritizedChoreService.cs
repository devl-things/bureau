using Niles.Chores;

namespace Niles.Chores.Abstractions.Services
{
    public interface IPrioritizedChoreService
    {
        Task<List<PrioritizedChore>> GetPrioritizedChoresAsync(DateOnly date, CancellationToken cancellationToken = default);
    }
}
