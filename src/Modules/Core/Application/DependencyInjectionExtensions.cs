using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

namespace WebSearchIndexing.Modules.Core.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCoreApplicationModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IScopedRequestSendingService, ScopedRequestSendingService>();
        services.AddHostedService<RequestSenderWorker>();

        return services;
    }
}
