using System.Text;
using AngularEnterpriseAPI.Services.Interfaces;

namespace AngularEnterpriseAPI.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IActivityService activityService)
        {
            // Skip audit for non-authenticated requests
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            var requestBody = await ReadRequestBody(context.Request);
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            var responseBodyContent = await ReadResponseBody(context.Response);

            // Log audit only for specific operations
            if (ShouldAudit(context.Request.Method))
            {
                var metadata = new Dictionary<string, object>
                {
                    ["method"] = context.Request.Method,
                    ["path"] = context.Request.Path,
                    ["statusCode"] = context.Response.StatusCode,
                    ["duration"] = stopwatch.ElapsedMilliseconds,
                    ["userAgent"] = context.Request.Headers["User-Agent"].ToString(),
                    ["ipAddress"] = GetClientIp(context)
                };

                if (!string.IsNullOrEmpty(requestBody) && context.Request.Method != "GET")
                {
                    metadata["requestBody"] = requestBody.Length > 1000
                        ? requestBody[..1000] + "..."
                        : requestBody;
                }

                await activityService.LogActivityAsync(
                    int.Parse(userId),
                    $"API_{context.Request.Method}",
                    $"API call to {context.Request.Path}",
                    GetClientIp(context),
                    context.Request.Headers["User-Agent"],
                    metadata
                );
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var buffer = new byte[1024 * 1024]; // 1MB max
            var readLength = await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer, 0, readLength);
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        private bool ShouldAudit(string method)
        {
            return method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH";
        }

        private string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"].ToString();

            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }
}
