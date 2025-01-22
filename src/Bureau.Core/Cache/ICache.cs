namespace Bureau.Cache
{
    public interface ICache
    {
        public string CreateCacheKey<T>(T request);
        public Task<string> CreateCacheKeyAsync<T>(T request, CancellationToken cancellationToken = default);
        public bool TryGetValue<T>(string key, out T cachedValue);
        public Task<bool> TryGetValueAsync<T>(string key, out T cachedValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets value in cache. If key already exists it'll override, if it doesn't it'll create new record.
        /// In any case it'll be with default options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="result"></param>
        public void SetValue<T>(string cacheKey, T result);
        public Task SetValueAsync<T>(string cacheKey, T result, CancellationToken cancellationToken = default);
    }
}
