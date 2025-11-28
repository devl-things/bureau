namespace Niles.Chores
{
    public interface ICriticalChoreService
    {
        Task<bool> CreateCriticalChoreAsync(int choreId, string note, CancellationToken cancellationToken = default);
        Task<bool> DeleteCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default);
        Task<bool> HasOpenCriticalChoreAsync(int choreId, CancellationToken cancellationToken = default);
        Task<List<int>> GetOpenCriticalChoreIdsAsync(List<int> choreIds, CancellationToken cancellationToken = default);
        Task MarkCriticalChoresAsCompletedAsync(Dictionary<int, int> choreIdToCompletedChoreIdMap, CancellationToken cancellationToken = default);
    }
}
