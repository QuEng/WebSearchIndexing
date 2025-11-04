using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Reporting.Application.Abstractions;
using WebSearchIndexing.Modules.Reporting.Application.Services;

namespace WebSearchIndexing.Modules.Reporting.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        services.AddScoped<IReportingQueryService, ReportingQueryService>();
        
        return services;
    }
}
