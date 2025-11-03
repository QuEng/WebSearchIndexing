using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Core.Application.BackgroundJobs;
using WebSearchIndexing.Modules.Core.Application.Commands.Processing;
using WebSearchIndexing.Modules.Core.Application.Commands.Settings;
using WebSearchIndexing.Modules.Core.Application.Queries.Settings;

namespace WebSearchIndexing.Modules.Core.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCoreApplicationModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IScopedRequestSendingService, ScopedRequestSendingService>();
        services.AddHostedService<RequestSenderWorker>();

        services.AddScoped<GetSettingsHandler>();
        services.AddScoped<UpdateSettingsHandler>();
        services.AddScoped<TriggerProcessingHandler>();

        return services;
    }
}
