using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Submission.Application.Abstractions;
using WebSearchIndexing.Modules.Submission.Application.Services;

namespace WebSearchIndexing.Modules.Submission.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSubmissionApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register application services
        services.AddScoped<ISubmissionService, SubmissionService>();

        return services;
    }
}
