using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Niles.Chores.Models;

namespace Niles.Chores
{
    internal static class IMemoryCacheExtensions
    {
        const string PRIORITIZED_CHORE_TOKEN_SOURCE_KEY = "pcTokenSource";

        public static void SetPriotizedChores(this IMemoryCache cache, YearsWeek key, List<PrioritizedChore> value)
        {
            CancellationTokenSource pcTokenSource = cache.GetOrCreate(PRIORITIZED_CHORE_TOKEN_SOURCE_KEY, _ => new CancellationTokenSource())!;
            MemoryCacheEntryOptions options = (new MemoryCacheEntryOptions()).AddExpirationToken(new CancellationChangeToken(pcTokenSource.Token));
            cache.Set(key, value, options);
        }

        public static void RemovePriotizedChores(this IMemoryCache cache)
        {
            if (cache.TryGetValue(PRIORITIZED_CHORE_TOKEN_SOURCE_KEY, out CancellationTokenSource? pcTokenSource))
            {
                pcTokenSource?.Cancel();
                cache.Remove(PRIORITIZED_CHORE_TOKEN_SOURCE_KEY);
            }
        }

        public static bool TryGetPriotizedChores(this IMemoryCache cache, YearsWeek key, out List<PrioritizedChore> value)
        {
            if (cache.TryGetValue(key, out List<PrioritizedChore>? list))
            {
                if (list is not null)
                {
                    value = list;
                    return true;
                }
            }
            value = [];
            return false;
        }
    }
}
