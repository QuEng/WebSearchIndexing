using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebSearchIndexing.BuildingBlocks.Observability;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        Action<ResourceBuilder>? configureResource = null,
        bool enableConsoleExporter = true,
        bool enableOtlpExporter = false)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOpenTelemetry()
            .ConfigureResource(rb =>
            {
                rb.AddService(serviceName: "WebSearchIndexing");
                configureResource?.Invoke(rb);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (enableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                if (enableOtlpExporter)
                {
                    tracing.AddOtlpExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (enableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                if (enableOtlpExporter)
                {
                    metrics.AddOtlpExporter();
                }
            })
            .WithLogging(
                configureBuilder: null,
                configureOptions: options =>
                {
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;
                    options.IncludeFormattedMessage = true;

                    if (enableConsoleExporter)
                    {
                        options.AddConsoleExporter();
                    }

                    if (enableOtlpExporter)
                    {
                        options.AddOtlpExporter();
                    }
                });

        return services;
    }
}
