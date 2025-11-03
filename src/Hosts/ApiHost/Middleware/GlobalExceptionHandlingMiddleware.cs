using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebSearchIndexing.Hosts.ApiHost.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing request {RequestPath}", 
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Don't write response if it has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot handle exception");
            return;
        }

        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();

        var (statusCode, title, detail) = GetErrorDetails(exception);

        var problemDetails = new ProblemDetails
        {
            Type = GetProblemTypeFromStatusCode((int)statusCode),
            Title = title,
            Detail = detail,
            Status = (int)statusCode,
            Instance = context.Request.Path.Value
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Include stack trace only in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private (HttpStatusCode statusCode, string title, string detail) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Bad Request", "Required parameter is missing"),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "Access denied"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Not Implemented", "This feature is not yet implemented"),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Request Timeout", "The request timed out"),
            
            // Domain-specific exceptions can be added here
            // e.g., DomainValidationException => (HttpStatusCode.BadRequest, "Domain Validation Failed", exception.Message),
            
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", 
                  _environment.IsDevelopment() ? exception.Message : "An internal server error occurred")
        };
    }

    private static string GetProblemTypeFromStatusCode(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        408 => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
