using RateLimitersMiddleware_NET6.Decorators;
using Microsoft.Extensions.Caching.Distributed;
using RateLimitersMiddleware_NET6.Extensions;
using System.Net;

namespace RateLimitersMiddleware_NET6.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public RateLimitingMiddleware(RequestDelegate next,IDistributedCache cache)
        {
            this._next = next;
            this._cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            // read the LimitRequest attribute from the endpoint
            var rateLimitDecorator = endpoint?.Metadata.GetMetadata<LimitRequest>();
            if (rateLimitDecorator is null)
            {
                await _next(context);
                return;
            }

            var key = GenerateClientKey(context);
            var _clientStatistics = GetClientStatisticsByKey(key).Result;

            // Check whether the request violates the rate limit policy
            if (_clientStatistics != null
                && DateTime.Now < _clientStatistics.LastSuccessfulResponseTime.AddSeconds(rateLimitDecorator.TimeWindow)
                && _clientStatistics.NumberofRequestsCompletedSuccessfully == rateLimitDecorator.MaxRequests)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                return;
            }
            await UpdateClientStatisticsAsync(key, rateLimitDecorator.MaxRequests);
            await _next(context);

        }

        /// <summary>
        /// generate ClientKey from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GenerateClientKey(HttpContext context)
         => $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";


        /// <summary>
        /// Get the client statistics from caching
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<ClientStatistics> GetClientStatisticsByKey(string key)
         => await _cache.GetCachedValueAsyn<ClientStatistics>(key);

        private async Task UpdateClientStatisticsAsync(string key, int maxRequests)
        {
            var _clientStats = _cache.GetCachedValueAsyn<ClientStatistics>(key).Result;
            if (_clientStats is not null)
            {
                _clientStats.LastSuccessfulResponseTime = DateTime.UtcNow;
                if (_clientStats.NumberofRequestsCompletedSuccessfully == maxRequests)
                    _clientStats.NumberofRequestsCompletedSuccessfully = 1;
                else
                    _clientStats.NumberofRequestsCompletedSuccessfully++;
                await _cache.SetCachedValueAsync<ClientStatistics>(key, _clientStats);
            }
            else
            {
                var clientStats = new ClientStatistics
                {
                    LastSuccessfulResponseTime = DateTime.UtcNow,
                    NumberofRequestsCompletedSuccessfully = 1
                };
                await _cache.SetCachedValueAsync<ClientStatistics>(key, clientStats);
            }
        }
    }
}
