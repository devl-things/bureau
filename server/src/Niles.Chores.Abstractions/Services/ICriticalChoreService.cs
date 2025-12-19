using Bureau;

namespace Niles.Chores.Services
{
    public interface ICriticalChoreService
    {
        Task<Result> CreateCriticalChoreAsync(int choreId, string note, CancellationToken cancellationToken = default);
        Task<Result> DeleteCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default);
        Task MarkCriticalChoresAsCompletedAsync(Dictionary<int, int> choreIdToCompletedChoreIdMap, CancellationToken cancellationToken = default);
    }
}
