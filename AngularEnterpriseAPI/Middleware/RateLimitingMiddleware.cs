using System.Collections.Concurrent;

namespace AngularEnterpriseAPI.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests = new();
        private readonly int _permitLimit;
        private readonly TimeSpan _window;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _permitLimit = configuration.GetValue<int>("RateLimitSettings:PermitLimit", 100);
            _window = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimitSettings:WindowInMinutes", 1));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIp(context);
            var key = $"{clientIp}:{context.Request.Path}";

            if (IsRateLimited(key))
            {
                _logger.LogWarning("Rate limit exceeded for IP {ClientIp} on path {Path}", clientIp, context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again later.",
                    statusCode = 429
                });
                return;
            }

            await _next(context);
        }

        private bool IsRateLimited(string key)
        {
            var now = DateTime.UtcNow;

            if (!_clientRequests.ContainsKey(key))
            {
                _clientRequests[key] = new ClientRequestInfo
                {
                    Count = 1,
                    FirstRequestTime = now,
                    LastRequestTime = now
                };
                return false;
            }

            var requestInfo = _clientRequests[key];

            if (now - requestInfo.FirstRequestTime > _window)
            {
                // Reset window
                requestInfo.Count = 1;
                requestInfo.FirstRequestTime = now;
                requestInfo.LastRequestTime = now;
                return false;
            }

            if (requestInfo.Count >= _permitLimit)
            {
                return true;
            }

            requestInfo.Count++;
            requestInfo.LastRequestTime = now;
            return false;
        }

        private string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"].ToString();

            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }

    public class ClientRequestInfo
    {
        public int Count { get; set; }
        public DateTime FirstRequestTime { get; set; }
        public DateTime LastRequestTime { get; set; }
    }
}
