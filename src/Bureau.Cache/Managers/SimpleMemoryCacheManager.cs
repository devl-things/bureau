using Bureau.Cache;
using Microsoft.Extensions.Logging;

namespace Bureau.Cache.Managers
{
    internal class SimpleMemoryCacheManager : ICache
    {
        ILogger<SimpleMemoryCacheManager> _logger;
        
        //TODO: SimpleMemoryCacheManager
        public SimpleMemoryCacheManager(ILogger<SimpleMemoryCacheManager> logger)
        {
            _logger = logger;
        }
        public string CreateCacheKey<T>(T request)
        {
            return "ONE_KEY_TO_RULE_THEM_ALL";
        }

        public Task<string> CreateCacheKeyAsync<T>(T request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("ONE_KEY_TO_RULE_THEM_ALL");
        }

        public void SetValue<T>(string cacheKey, T result)
        {
        }

        public Task SetValueAsync<T>(string cacheKey, T result, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public bool TryGetValue<T>(string key, out T cachedValue)
        {
            cachedValue = default!;
            return false;
        }

        public Task<bool> TryGetValueAsync<T>(string key, out T cachedValue, CancellationToken cancellationToken = default)
        {
            cachedValue = default!;
            return Task.FromResult(false);
        }
    }
}
