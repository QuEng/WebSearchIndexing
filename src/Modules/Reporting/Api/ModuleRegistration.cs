using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Reporting.Api.Endpoints;
using WebSearchIndexing.Modules.Reporting.Application;

namespace WebSearchIndexing.Modules.Reporting.Api;

public static class ReportingModule
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddReportingApplication();
        
        return services;
    }

    public static IEndpointRouteBuilder MapReportingModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var reportingGroup = endpoints.MapGroup("/api/reporting");

        reportingGroup.MapStatsEndpoints();

        return endpoints;
    }
}
