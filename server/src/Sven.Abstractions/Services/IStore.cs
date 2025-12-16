using Bureau;

namespace Sven.Services
{
    public interface IStore<TKey, TValue> where TKey : notnull
    {
        bool Exists(TKey key);
        Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken = default);
        Task<Result> StoreAsync(TKey key, TValue request, CancellationToken cancellationToken = default);
        Task<Result<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default);
        Task<Result> RemoveAsync(TKey key, CancellationToken cancellationToken = default);
    }
}
