using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Notifications.Api;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}
