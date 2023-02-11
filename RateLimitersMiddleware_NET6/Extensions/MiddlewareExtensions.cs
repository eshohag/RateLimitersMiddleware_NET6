
using RateLimitersMiddleware_NET6.Middlewares;

namespace RateLimitersMiddleware_NET6.Extensions
{
    public static class MiddlewareExtensions
    {

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
