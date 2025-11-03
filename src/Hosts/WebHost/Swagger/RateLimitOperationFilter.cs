using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebSearchIndexing.Hosts.WebHost.Swagger;

/// <summary>
/// Swagger operation filter that adds rate limiting information to API operations
/// </summary>
public class RateLimitOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add rate limiting headers to all responses
        foreach (var response in operation.Responses.Values)
        {
            response.Headers ??= new Dictionary<string, OpenApiHeader>();
            
            if (!response.Headers.ContainsKey("X-RateLimit-Limit"))
            {
                response.Headers.Add("X-RateLimit-Limit", new OpenApiHeader
                {
                    Description = "Request limit per time window",
                    Schema = new OpenApiSchema { Type = "integer" }
                });
            }
            
            if (!response.Headers.ContainsKey("X-RateLimit-Remaining"))
            {
                response.Headers.Add("X-RateLimit-Remaining", new OpenApiHeader
                {
                    Description = "Remaining requests in current time window",
                    Schema = new OpenApiSchema { Type = "integer" }
                });
            }
            
            if (!response.Headers.ContainsKey("X-RateLimit-Reset"))
            {
                response.Headers.Add("X-RateLimit-Reset", new OpenApiHeader
                {
                    Description = "Time when the rate limit resets (Unix timestamp)",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int64" }
                });
            }
        }

        // Add 429 Too Many Requests response if not already present
        if (!operation.Responses.ContainsKey("429"))
        {
            operation.Responses.Add("429", new OpenApiResponse
            {
                Description = "Too Many Requests",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["type"] = new OpenApiSchema { Type = "string" },
                                ["title"] = new OpenApiSchema { Type = "string" },
                                ["status"] = new OpenApiSchema { Type = "integer" },
                                ["detail"] = new OpenApiSchema { Type = "string" },
                                ["instance"] = new OpenApiSchema { Type = "string" },
                                ["correlationId"] = new OpenApiSchema { Type = "string" },
                                ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time" }
                            }
                        }
                    }
                },
                Headers = new Dictionary<string, OpenApiHeader>
                {
                    ["Retry-After"] = new OpenApiHeader
                    {
                        Description = "Number of seconds to wait before retrying",
                        Schema = new OpenApiSchema { Type = "integer" }
                    }
                }
            });
        }
    }
}
