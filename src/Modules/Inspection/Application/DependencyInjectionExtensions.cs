using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Inspection.Application.Services;

namespace WebSearchIndexing.Modules.Inspection.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInspectionApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register application services
        services.AddScoped<IInspectionService, InspectionService>();

        return services;
    }
}
