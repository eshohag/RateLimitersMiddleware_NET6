
using Microsoft.Extensions.Caching.Distributed;

namespace RateLimitersMiddleware_NET6.Extensions
{
    public static class DistributedCacheExtensions
    {
        public async static Task<T> GetCachedValueAsyn<T>(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken)) where T : class
        {
            var result = await cache.GetAsync(key, token);
            return result.FromByteArray<T>();
        }

        public async static Task SetCachedValueAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken token = default(CancellationToken))
        {
            await cache.SetAsync(key, value.ToByteArray(), token);
        }
    }

}
