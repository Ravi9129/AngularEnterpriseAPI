using System.Diagnostics;

namespace AngularEnterpriseAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log request
            _logger.LogInformation(
                "Request: {Method} {Path} - Started at {Time}",
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            await _next(context);

            stopwatch.Stop();

            // Log response
            _logger.LogInformation(
                "Response: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
