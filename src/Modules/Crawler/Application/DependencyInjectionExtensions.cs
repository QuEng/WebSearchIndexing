using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Crawler.Application.Abstractions;
using WebSearchIndexing.Modules.Crawler.Application.Services;

namespace WebSearchIndexing.Modules.Crawler.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCrawlerApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register application services
        services.AddScoped<ICrawlerService, CrawlerService>();

        // Register HttpClient for URL verification
        services.AddHttpClient<CrawlerService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "WebSearchIndexing-Crawler/1.0");
        });

        return services;
    }
}
