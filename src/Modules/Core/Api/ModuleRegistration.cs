using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Core.Application;

namespace WebSearchIndexing.Modules.Core.Api;

public static class CoreModule
{
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCoreApplicationModule();
        return services;
    }

    public static IEndpointRouteBuilder MapCoreModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var coreGroup = endpoints.MapGroup("/api/v1/core");
        coreGroup.MapSettingsEndpoints();

        return endpoints;
    }
}
