using Bureau.Cache.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Cache.Configurations
{
    public static class CacheServiceConfiguration
    {
        public static void AddBureauCache(this IServiceCollection services)
        {
            services.AddSingleton<ICache, SimpleMemoryCacheManager>();
        }
    }
}
