using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.BuildingBlocks.Observability;
using WebSearchIndexing.Hosts.ApiHost.Extensions;
using WebSearchIndexing.Hosts.ApiHost.Middleware;
using WebSearchIndexing.Hosts.ApiHost.Swagger;
using WebSearchIndexing.Modules.Catalog.Api;
using WebSearchIndexing.Modules.Core.Api;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Crawler.Api;
using WebSearchIndexing.Modules.Inspection.Api;
using WebSearchIndexing.Modules.Notifications.Api;
using WebSearchIndexing.Modules.Reporting.Api;
using WebSearchIndexing.Modules.Submission.Api;

var builder = WebApplication.CreateBuilder(args);

// Serilog basic setup
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("IndexingDb")!, name: "postgres");

// API-specific services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.HeaderApiVersionReader("X-API-Version"),
        new Asp.Versioning.QueryStringApiVersionReader("version"),
        new Asp.Versioning.UrlSegmentApiVersionReader()
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    // Configure versioning
    var provider = builder.Services.BuildServiceProvider().GetService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    if (provider != null)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "WebSearchIndexing API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ?
                    "API for Web Search Indexing system - Backend for WASM client (DEPRECATED)" :
                    "API for Web Search Indexing system - Backend for WASM client",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@websearchindexing.com"
                }
            });
        }
    }
    else
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "WebSearchIndexing API",
            Version = "v1",
            Description = "API for Web Search Indexing system - Backend for WASM client",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Development Team",
                Email = "dev@websearchindexing.com"
            }
        });
    }

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add operation filters for enhanced API documentation
    options.OperationFilter<CorrelationIdOperationFilter>();
    options.OperationFilter<RateLimitOperationFilter>();
});

// CORS for WASM client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5124",  // WASM host development HTTP
                "https://localhost:5124", // WASM host development HTTPS
                "https://localhost:7001"  // WASM host production
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "WebSearchIndexing.ApiHost",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "WebSearchIndexing.WasmHost",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-secret-key-min-32-chars-long"))
        };

        // Enable JWT events for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Rate Limiting for API
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("api", key => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1000,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 100
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

// Problem Details for consistent error handling
builder.Services.AddProblemDetails();

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

// Infrastructure and modules
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddCoreModule()
    .AddCatalogModule()
    .AddSubmissionModule()
    .AddInspectionModule()
    .AddCrawlerModule()
    .AddNotificationsModule()
    .AddReportingModule()
    .AddCoreApplicationModule();

// Add outbox background service for processing integration events
builder.Services.AddOutboxBackgroundService();

var app = builder.Build();

app.ApplyMigrations();
app.UseMultiTenant();

// Add global exception handling middleware early in pipeline (but after multi-tenant)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Add correlation ID middleware early in the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        if (provider != null)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    $"WebSearchIndexing API {description.GroupName.ToUpperInvariant()}");
            }
        }
        else
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSearchIndexing API v1");
        }

        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
    });
}

// Important: CORS before authentication
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

// Health checks
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Map all module endpoints
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
