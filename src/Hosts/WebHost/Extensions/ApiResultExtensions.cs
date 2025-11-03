using Microsoft.AspNetCore.Mvc;

namespace WebSearchIndexing.Hosts.WebHost.Extensions;

/// <summary>
/// Extension methods for consistent API error handling
/// </summary>
public static class ApiResultExtensions
{
    /// <summary>
    /// Creates a consistent error response with correlation ID
    /// </summary>
    public static IResult ErrorResponse(
        this HttpContext context,
        string title,
        string detail,
        int statusCode = 400,
        string? instance = null)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        var problemDetails = new ProblemDetails
        {
            Type = GetProblemTypeFromStatusCode(statusCode),
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = instance ?? context.Request.Path.Value
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        return Results.Problem(problemDetails);
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static IResult ValidationErrorResponse(
        this HttpContext context,
        Dictionary<string, string[]> errors,
        string title = "Validation failed")
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        var validationProblem = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = title,
            Status = 400,
            Instance = context.Request.Path.Value
        };

        validationProblem.Extensions["correlationId"] = correlationId;
        validationProblem.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        return Results.ValidationProblem(errors, title, statusCode: 400, 
            instance: context.Request.Path.Value);
    }

    /// <summary>
    /// Creates a not found error response
    /// </summary>
    public static IResult NotFoundResponse(
        this HttpContext context,
        string resourceType,
        string identifier)
    {
        return context.ErrorResponse(
            title: "Resource not found",
            detail: $"{resourceType} with identifier '{identifier}' was not found",
            statusCode: 404);
    }

    /// <summary>
    /// Creates an internal server error response
    /// </summary>
    public static IResult InternalServerErrorResponse(
        this HttpContext context,
        Exception ex,
        bool includeDetails = false)
    {
        var detail = includeDetails 
            ? $"An error occurred: {ex.Message}" 
            : "An internal server error occurred";

        return context.ErrorResponse(
            title: "Internal Server Error",
            detail: detail,
            statusCode: 500);
    }

    /// <summary>
    /// Creates a rate limit exceeded response
    /// </summary>
    public static IResult RateLimitExceededResponse(
        this HttpContext context,
        TimeSpan retryAfter)
    {
        var result = context.ErrorResponse(
            title: "Rate limit exceeded",
            detail: "Too many requests. Please try again later.",
            statusCode: 429);

        context.Response.Headers.TryAdd("Retry-After", ((int)retryAfter.TotalSeconds).ToString());
        
        return result;
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeaderName = "X-Correlation-ID";
        
        var correlationId = context.Request.Headers[correlationIdHeaderName].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        return correlationId;
    }

    private static string GetProblemTypeFromStatusCode(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
