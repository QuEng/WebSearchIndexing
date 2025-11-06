using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Submission.Api;

/// <summary>
/// Submission module registration extensions
/// </summary>
public static class SubmissionModule
{
    /// <summary>
    /// Adds submission module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSubmissionModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    /// <summary>
    /// Maps submission module endpoints to the route builder
    /// </summary>
    /// <param name="endpoints">Endpoint route builder</param>
    /// <returns>Endpoint route builder for chaining</returns>
    public static IEndpointRouteBuilder MapSubmissionModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var submissonGroup = endpoints.MapGroup("/api/v1/submission");

        return endpoints;
    }
}
