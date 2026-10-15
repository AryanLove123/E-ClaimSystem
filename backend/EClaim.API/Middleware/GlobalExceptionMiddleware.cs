using System.Net;
using System.Text.Json;
using EClaim.Application.Common;
using EClaim.Domain.Exceptions;
using EClaim.Infrastructure.Logging;

namespace Eclaim.API.Middleware;

public class GlobalExceptionMiddleware
{
    private RequestDelegate _next;
    private ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            try
            {
                var dbLogger = context.RequestServices.GetService<DatabaseLoggerService>();
                if(dbLogger != null)
                {
                    int? userId = null;
                    var idClaim = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (idClaim != null && int.TryParse(idClaim, out var parsed)) userId = parsed;

                    await dbLogger.LogErrorAsync(ex, traceId, context.Request.Path, userId);
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write ErrorLog to database.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                ValidationException => (int)HttpStatusCode.BadRequest,
                ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
                InvalidWorkflowTransitionException => (int)HttpStatusCode.Conflict,
                DomainException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = ex is DomainException ? ex.Message : "An unexpected error occurred.";
            var errors =  ex is ValidationException ve && ve.Errors.Any()
                ? ve.Errors.SelectMany(kv => kv.Value).ToList()
                : new List<string>();
            
            var response = ApiResponse<object>.Fail(message, errors, traceId);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase}));
        }
    }
}