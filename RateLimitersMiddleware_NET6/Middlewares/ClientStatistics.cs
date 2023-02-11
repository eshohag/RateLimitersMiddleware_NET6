namespace RateLimitersMiddleware_NET6.Middlewares
{
    public class ClientStatistics
    {
        public DateTime LastSuccessfulResponseTime { get; set; }
        public int NumberofRequestsCompletedSuccessfully { get; set; }
    }
}
