using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Reporting.Api;

public static class ReportingModule
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    public static IEndpointRouteBuilder MapReportingModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}
