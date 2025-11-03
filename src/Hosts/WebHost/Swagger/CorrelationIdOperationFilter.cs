using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebSearchIndexing.Hosts.WebHost.Swagger;

/// <summary>
/// Swagger operation filter that adds correlation ID parameter to all API operations
/// </summary>
public class CorrelationIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            },
            Description = "Correlation ID for request tracking. If not provided, one will be generated automatically."
        });
    }
}
