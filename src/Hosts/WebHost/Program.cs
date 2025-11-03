using Finbuckle.MultiTenant;
using MudBlazor.Services;
using Serilog;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.BuildingBlocks.Observability;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Hosts.WebHost.Components;
using WebSearchIndexing.Hosts.WebHost.Extensions;
using WebSearchIndexing.Hosts.WebHost.Navigation;
using WebSearchIndexing.Hosts.WebHost.Swagger;
using WebSearchIndexing.Hosts.WebHost.Middleware;
using WebSearchIndexing.Modules.Catalog.Api;
using WebSearchIndexing.Modules.Catalog.Ui;
using WebSearchIndexing.Modules.Core.Api;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Ui;
using WebSearchIndexing.Modules.Crawler.Api;
using WebSearchIndexing.Modules.Inspection.Api;
using WebSearchIndexing.Modules.Notifications.Api;
using WebSearchIndexing.Modules.Reporting.Api;
using WebSearchIndexing.Modules.Reporting.Ui;
using WebSearchIndexing.Modules.Submission.Api;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog basic setup
Log.Logger = new LoggerConfiguration()
 .ReadFrom.Configuration(builder.Configuration)
 .Enrich.FromLogContext()
 .WriteTo.Console()
 .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHealthChecks()
 .AddNpgSql(builder.Configuration.GetConnectionString("IndexingDb")!, name: "postgres");

builder.Services.AddWebSupport();
builder.Services.AddMudServices();

// Data Protection (for secrets encryption)
builder.Services.AddDataProtection();

// Rate Limiting (simplified implementation)
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiting for API endpoints
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/api"))
        {
            return RateLimitPartition.GetFixedWindowLimiter("api", key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
        }
        return RateLimitPartition.GetNoLimiter("unlimited");
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = 
                ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WebSearchIndexing API",
        Version = "v1",
        Description = "API for Web Search Indexing system",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@websearchindexing.com"
        }
    });

    // Add correlation ID parameter to all operations
    options.OperationFilter<CorrelationIdOperationFilter>();
    
    // Add rate limiting information to operations
    options.OperationFilter<RateLimitOperationFilter>();
});

// Problem Details for consistent error handling
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Add correlation ID to all problem details
        if (context.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            context.ProblemDetails.Extensions["correlationId"] = correlationId.ToString();
        }
        else
        {
            context.ProblemDetails.Extensions["correlationId"] = Guid.NewGuid().ToString();
        }

        context.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
        
        // Add instance path
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    };
});

// Observability (OTEL)
builder.Services.AddObservability();

var connectionString = builder.Configuration.GetConnectionString("IndexingDb");

// Multi-tenant: default in-memory store with a default tenant
builder.Services
 .AddMultiTenant<TenantInfo>()
 .WithInMemoryStore(options =>
 {
     options.Tenants.Add(new TenantInfo
     {
         Id = Guid.Empty.ToString(),
         Identifier = "default",
         Name = "Default",
         ConnectionString = connectionString
     });
 })
 .WithStaticStrategy("default");

builder.Services
 .AddRazorComponents()
 .AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<INavigationContributor, WebHostNavigationContributor>();

// Configure HttpClient for Blazor Server scenario
builder.Services.AddHttpClient("WebSearchIndexing.Api", client =>
{
    // In Blazor Server, this will be the local API
    // In WASM, this should be configured to point to the API server
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services
 .AddCoreModule()
 .AddCatalogModule()
 .AddSubmissionModule()
 .AddInspectionModule()
 .AddCrawlerModule()
 .AddNotificationsModule()
 .AddReportingModule()
 .AddCoreApplicationModule()
 .AddCoreUiModule()
 .AddCatalogUiModule()
 .AddReportingUiModule();

// Add outbox background service for processing integration events
builder.Services.AddOutboxBackgroundService();

var app = builder.Build();

app.ApplyMigrations();
app.UseMultiTenant();

// Add global exception handling middleware early in pipeline (but after multi-tenant)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Add correlation ID middleware early in the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

// Add rate limiting
app.UseRateLimiter();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    // Swagger only in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSearchIndexing API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
    });
}

app.UseStaticFiles();
app.UseAntiforgery();

var componentAssemblies = app.Services
 .GetRequiredService<IEnumerable<IRazorComponentAssemblyProvider>>()
 .Select(provider => provider.Assembly)
 .Where(assembly => assembly != typeof(App).Assembly)
 .ToArray();

app.MapRazorComponents<App>()
 .AddInteractiveServerRenderMode()
 .AddAdditionalAssemblies(componentAssemblies);

app.MapHealthChecks("/health/live");
// Readiness endpoint (extend with per-dependency checks if needed)
app.MapHealthChecks("/health/ready");

app.MapCoreModuleEndpoints();
app.MapCatalogModuleEndpoints();
app.MapSubmissionModuleEndpoints();
app.MapInspectionModuleEndpoints();
app.MapCrawlerModuleEndpoints();
app.MapNotificationsModuleEndpoints();
app.MapReportingModuleEndpoints();

app.Run();

// Make Program class accessible to tests
public partial class Program { }
