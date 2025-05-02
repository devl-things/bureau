using Bureau.Core;
using System.Collections.Concurrent;

namespace Sven.Services
{
    public class InMemoryStore<TKey, TValue> : IStore<TKey, TValue> where TKey : notnull
    {
        protected readonly ConcurrentDictionary<TKey, TValue> _store = new();

        public bool Exists(TKey key)
        {
            return _store.ContainsKey(key);
        }

        public Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists(key));
        }

        public Task<Result<TValue>> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            if (key is null)
            {
                return Task.FromResult(new Result<TValue>(new ResultError()));
            }
            bool isFound = _store.TryGetValue(key, out TValue? result);
            return Task.FromResult(new Result<TValue>(result, isFound));
        }

        public Task<Result> RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            _store.TryRemove(key, out _);
            return Task.FromResult(new Result());
        }

        public Task<Result> StoreAsync(TKey key, TValue request, CancellationToken cancellationToken = default)
        {
            _store[key] = request;
            return Task.FromResult(new Result());
        }
    }
}
