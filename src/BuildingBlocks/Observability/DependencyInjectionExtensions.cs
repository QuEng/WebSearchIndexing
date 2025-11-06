using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace WebSearchIndexing.BuildingBlocks.Observability;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ResourceBuilder>? configureResource = null,
        bool enableConsoleExporter = true,
        bool enableOtlpExporter = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure Serilog for structured logging to Seq
        var seqUrl = configuration["Seq:ServerUrl"] ?? "http://localhost:5341";
        var seqApiKey = configuration["Seq:ApiKey"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "WebSearchIndexing")
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl, apiKey: seqApiKey)
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddOpenTelemetry()
            .ConfigureResource(rb =>
            {
                rb.AddService(
                    serviceName: "WebSearchIndexing",
                    serviceVersion: typeof(DependencyInjectionExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0");
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
            });

        return services;
    }
}
