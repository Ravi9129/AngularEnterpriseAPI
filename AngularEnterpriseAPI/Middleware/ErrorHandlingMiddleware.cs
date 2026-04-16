using System.Net;
using System.Text.Json;
using AngularEnterpriseAPI.DTOs.Common;

namespace AngularEnterpriseAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while processing your request",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    apiResponse.Message = exception.Message;
                    apiResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    apiResponse.Message = exception.Message;
                    apiResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse.Message = exception.Message;
                    apiResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    apiResponse.Message = "An internal server error occurred";
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(apiResponse);
            await response.WriteAsync(jsonResponse);
        }
    }
}
