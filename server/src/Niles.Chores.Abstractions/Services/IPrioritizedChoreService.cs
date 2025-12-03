namespace Niles.Chores.Services
{
    public interface IPrioritizedChoreService
    {
        Task<List<PrioritizedChore>> GetPrioritizedChoresAsync(DateOnly date, CancellationToken cancellationToken = default);
    }
}
