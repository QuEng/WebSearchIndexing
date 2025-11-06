using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Ui.Services;
using WebSearchIndexing.Modules.Identity.Ui.State;

namespace WebSearchIndexing.Modules.Identity.Ui;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddIdentityUIModule(this IServiceCollection services)
    {
        // Register token storage as singleton to maintain state across components
        services.AddSingleton<ISecureTokenStorage, SecureTokenStorage>();

        // Register authentication services
        services.AddScoped<HybridAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<HybridAuthenticationStateProvider>());

        services.AddScoped<HybridAuthService>();

        // Register state management
        services.AddScoped<IdentityStateManager>();

        return services;
    }

    public static IServiceCollection AddIdentityHttpClient(this IServiceCollection services, string baseAddress)
    {
        // Register the interceptor within the HttpClient configuration
        services.AddTransient<SmartHttpClientInterceptor>();
        
        // Auth client without interceptor (to avoid circular dependency)
        services.AddHttpClient("AuthApi", client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        // General API client with interceptor
        services.AddHttpClient("IdentityApi", client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<SmartHttpClientInterceptor>();

        // Register named HttpClient for dependency injection (general API calls)
        services.AddScoped(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            return httpClientFactory.CreateClient("IdentityApi");
        });

        return services;
    }
}
