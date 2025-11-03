# План міграції WebSearchIndexing до Blazor WASM

## Огляд міграції

Цей документ описує повну міграцію проекту WebSearchIndexing з Blazor Server на Blazor WebAssembly (WASM) з збереженням всієї поточної функціональності та модульної архітектури.

## Поточна архітектура (Blazor Server)

### Структура проекту
```
src/
├── BuildingBlocks/              # Спільні компоненти
│   ├── Abstractions/           # Інтерфейси та контракти
│   ├── Messaging/              # Outbox pattern, події
│   ├── Observability/          # OpenTelemetry, Serilog
│   ├── Persistence/            # EF Core базові класи
│   └── Web/                    # Blazor компоненти, навігація
├── Contracts/                  # API контракти
├── Hosts/
│   └── WebHost/               # Blazor Server хост
├── Modules/                   # Бізнес модулі
│   ├── Core/                  # Налаштування, базова функціональність
│   ├── Catalog/               # Управління сайтами та аккаунтами
│   ├── Reporting/             # Звіти та дашборд
│   ├── Submission/            # Подача URL в Google Indexing API
│   ├── Inspection/            # Перевірка статусів URL
│   ├── Crawler/               # Сканування сайтів
│   └── Notifications/         # Сповіщення
└── Workers/                   # Background services
```

### Поточні технології
- **Framework:** .NET 10.0 → потрібно .NET 9.0
- **UI:** Blazor Server + MudBlazor 6.19.1
- **Database:** PostgreSQL + EF Core 8.0.21
- **Multi-tenancy:** Finbuckle.MultiTenant 6.13.1
- **Architecture:** Clean Architecture, CQRS, Outbox Pattern

## Цільова архітектура (Blazor WASM)

### Нова структура
```
src/
├── BuildingBlocks/              # Без змін
├── Contracts/                  # Розширені API контракти
├── Hosts/
│   ├── ApiHost/               # Новий - ASP.NET Core Web API
│   └── WasmHost/              # Новий - Blazor WASM клієнт
├── Modules/
│   ├── [Module]/
│   │   ├── Api/              # API endpoints (переміщені в ApiHost)
│   │   ├── Application/      # Без змін
│   │   ├── Domain/           # Без змін
│   │   ├── Infrastructure/   # Тільки для ApiHost
│   │   └── Ui/              # WASM-сумісні компоненти
└── Workers/                   # Без змін
```

### Розділення відповідальностей

#### ApiHost (Backend)
- **Призначення:** REST API сервер
- **Технології:** ASP.NET Core Web API, EF Core
- **Відповідальності:**
  - Обробка HTTP запитів
  - Бізнес логіка через Application layer
  - Доступ до бази даних
  - Аутентифікація (JWT)
  - Мультитенантність
  - Background services (Outbox processing)

#### WasmHost (Frontend)
- **Призначення:** Blazor WASM клієнт
- **Технології:** Blazor WebAssembly, MudBlazor
- **Відповідальності:**
  - UI рендеринг в браузері
  - Client-side навігація
  - HTTP комунікація з API
  - Client-side кешування
  - Аутентифікація (токени)

## Детальний план міграції

## ФАЗА 1: Підготовка (.NET 9 Migration)

### Етап 1.1: Downgrade до .NET 9
**Час:** 1 тиждень

**Зміни:**
1. **Directory.Build.props:**
   ```xml
   <PropertyGroup>
     <TargetFramework>net9.0</TargetFramework>
   </PropertyGroup>
   ```

2. **Оновлення пакетів:**
   - `Microsoft.EntityFrameworkCore` → 9.0.x
   - `Microsoft.AspNetCore.*` → 9.0.x
   - Перевірка сумісності всіх NuGet пакетів

3. **Тестування:** Повна компіляція та функціональні тести

**Критерії прийняття:**
- ✅ Проект компілюється без помилок
- ✅ Всі модулі працюють на .NET 9
- ✅ Міграції БД виконуються без помилок

### Етап 1.2: Аналіз WASM сумісності
**Час:** 3 дні

**Завдання:**
1. Перевірити сумісність MudBlazor з WASM
2. Ідентифікувати server-only залежності
3. Підготувати список змін для кожного UI модуля

**Результат:** Документ з переліком несумісних компонентів

## ФАЗА 2: Створення API Host

### Етап 2.1: Новий ApiHost проект
**Час:** 1 тиждень

**Створення проекту:**
```csharp
// src/Hosts/ApiHost/Program.cs
var builder = WebApplication.CreateBuilder(args);

// API-specific services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for WASM client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7001") // WASM host
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // JWT configuration
    });

// All existing modules
builder.Services
    .AddCoreModule()
    .AddCatalogModule()
    .AddReportingModule()
    // ... інші модулі
    ;

var app = builder.Build();

// Middleware pipeline
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map all module endpoints
app.MapCoreModuleEndpoints();
app.MapCatalogModuleEndpoints();
// ... інші endpoints

app.Run();
```

**Критерії прийняття:**
- ✅ API доступний на окремому порту
- ✅ Всі module endpoints працюють
- ✅ Swagger документація доступна
- ✅ CORS налаштований для WASM

### Етап 2.2: Міграція endpoints з WebHost
**Час:** 3 дні

**Процес:**
1. Перемістити всі `MapXxxModuleEndpoints()` в ApiHost
2. Видалити API endpoints з WebHost
3. Налаштувати middleware stack для API
4. Додати API versioning

**Критерії прийняття:**
- ✅ Всі API endpoints доступні в ApiHost
- ✅ WebHost містить тільки Blazor Server UI
- ✅ API tests проходять

## ФАЗА 3: Підготовка UI для WASM

### Етап 3.1: Модифікація UI проектів
**Час:** 2 тижні

**Зміни в кожному UI модулі:**

1. **Оновлення .csproj:**
   ```xml
   <!-- Було -->
   <FrameworkReference Include="Microsoft.AspNetCore.App" />
   
   <!-- Стало -->
   <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
   ```

2. **Створення HTTP клієнтів:**
   ```csharp
   // src/Modules/Core/Ui/Services/CoreApiClient.cs
   public class CoreApiClient
   {
       private readonly HttpClient _httpClient;
       
       public CoreApiClient(HttpClient httpClient)
       {
           _httpClient = httpClient;
       }
       
       public async Task<SettingsDto> GetSettingsAsync()
       {
           var response = await _httpClient.GetAsync("/api/v1/core/settings");
           return await response.Content.ReadFromJsonAsync<SettingsDto>();
       }
   }
   ```

3. **Оновлення DI реєстрації:**
   ```csharp
   // src/Modules/Core/Ui/DependencyInjection.cs
   public static class CoreUiModule
   {
       public static IServiceCollection AddCoreUiModule(this IServiceCollection services)
       {
           // Замість прямого доступу до репозиторіїв
           services.AddHttpClient<CoreApiClient>(client => 
           {
               client.BaseAddress = new Uri("https://api.domain.com/");
           });
           
           return services;
       }
   }
   ```

**Модулі для оновлення:**
- Core.Ui
- Catalog.Ui  
- Reporting.Ui

**Критерії прийняття:**
- ✅ UI модулі компілюються без server dependencies
- ✅ HTTP клієнти створені для всіх потрібних API
- ✅ DI налаштований для WASM

### Етап 3.2: Видалення server-specific коду
**Час:** 1 тиждень

**Замінити/видалити:**
- `IHttpContextAccessor` → альтернативні рішення
- Server-side authentication → token-based
- Direct database access → HTTP API calls
- Server-side file operations → client-side + API

**Критерії прийняття:**
- ✅ Немає посилань на server-only APIs
- ✅ Всі UI компоненти WASM-сумісні

## ФАЗА 4: Створення WASM Host

### Етап 4.1: Базовий WASM проект
**Час:** 1 тиждень

**Створення WasmHost:**
```csharp
// src/Hosts/WasmHost/Program.cs
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP client для API
builder.Services.AddHttpClient("WebSearchIndexing.Api", client => 
{
    client.BaseAddress = new Uri("https://api.domain.com/");
});

// MudBlazor
builder.Services.AddMudServices();

// UI модулі
builder.Services
    .AddCoreUiModule()
    .AddCatalogUiModule()
    .AddReportingUiModule();

await builder.Build().RunAsync();
```

**Структура:**
```
src/Hosts/WasmHost/
├── Program.cs                  # Entry point
├── App.razor                   # Root component  
├── wwwroot/
│   ├── index.html             # Host page
│   ├── css/                   # Styles
│   ├── js/                    # JS interop
│   └── favicon.ico
├── Components/
│   ├── Layout/                # Shared layouts
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/                 # Route pages
│       ├── Home.razor
│       └── Error.razor
└── Services/                  # WASM-specific services
    ├── AuthService.cs
    └── TenantService.cs
```

**Критерії прийняття:**
- ✅ WASM додаток запускається
- ✅ Основна навігація працює
- ✅ UI модулі підключені

### Етап 4.2: Підключення UI модулів
**Час:** 1 тиждень

**Процес:**
1. Додати модульні assemblies до Router
2. Налаштувати lazy loading
3. Перевірити всі маршрути

```csharp
// App.razor
<Router AppAssembly="@typeof(App).Assembly" 
        AdditionalAssemblies="@_moduleAssemblies"
        OnNavigateAsync="@OnNavigateAsync">
    <!-- Router content -->
</Router>

@code {
    private Assembly[] _moduleAssemblies = new[]
    {
        typeof(CoreUiModule).Assembly,
        typeof(CatalogUiModule).Assembly,
        typeof(ReportingUiModule).Assembly
    };
}
```

**Критерії прийняття:**
- ✅ Всі сторінки модулів доступні
- ✅ Навігація між модулями працює
- ✅ Компоненти рендеряться коректно

## ФАЗА 5: Аутентифікація та безпека

### Етап 5.1: JWT аутентифікація в API
**Час:** 1 тиждень

**Налаштування API:**
```csharp
// ApiHost/Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

**API endpoints:**
```csharp
// Login endpoint
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    if (result.Success)
    {
        return Results.Ok(new { token = result.Token });
    }
    return Results.Unauthorized();
});
```

**Критерії прийняття:**
- ✅ JWT токени генеруються
- ✅ API endpoints захищені
- ✅ Token validation працює

### Етап 5.2: Аутентифікація в WASM
**Час:** 1 тиждень

**WASM auth service:**
```csharp
// WasmHost/Services/AuthService.cs
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", 
            new { Username = username, Password = password });
            
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "token", result.Token);
            return true;
        }
        return false;
    }
}
```

**HTTP interceptor:**
```csharp
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

**Критерії прийняття:**
- ✅ Користувач може увійти через WASM
- ✅ Токени автоматично додаються до запитів
- ✅ Logout працює

## ФАЗА 6: Мультитенантність

### Етап 6.1: Tenant handling в API
**Час:** 1 тиждень

**Поточне рішення:** Finbuckle.MultiTenant залишається в API

**Middleware для tenant detection:**
```csharp
public class TenantMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Detect tenant from header, subdomain, etc.
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault()
                    ?? DetectFromSubdomain(context.Request.Host);
                    
        await tenantService.SetCurrentTenantAsync(tenantId);
        await _next(context);
    }
}
```

**Критерії прийняття:**
- ✅ Tenant isolation працює в API
- ✅ EF Core фільтри застосовуються
- ✅ API endpoints tenant-aware

### Етап 6.2: Tenant management в WASM
**Час:** 3 дні

**Client-side tenant service:**
```csharp
// WasmHost/Services/TenantService.cs
public class TenantService
{
    private readonly IJSRuntime _jsRuntime;
    private string? _currentTenantId;
    
    public async Task SetTenantAsync(string tenantId)
    {
        _currentTenantId = tenantId;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tenantId", tenantId);
    }
    
    public string GetCurrentTenantId() => _currentTenantId ?? "default";
}
```

**HTTP клієнт оновлення:**
```csharp
services.AddHttpClient<ApiClient>(client => {
    client.BaseAddress = new Uri("https://api.domain.com/");
}).AddHttpMessageHandler<TenantMessageHandler>();

public class TenantMessageHandler : DelegatingHandler
{
    private readonly TenantService _tenantService;
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Tenant-Id", _tenantService.GetCurrentTenantId());
        return await base.SendAsync(request, cancellationToken);
    }
}
```

**Критерії прийняття:**
- ✅ WASM client передає tenant context
- ✅ Дані ізольовані по tenant
- ✅ UI відображає правильний tenant

## ФАЗА 7: Файлова функціональність

### Етап 7.1: Оновлення file operations
**Час:** 1 тиждень

**Проблеми для вирішення:**
1. CSV імпорт в Catalog модулі
2. Експорт звітів
3. Завантаження конфігурацій

**Рішення для імпорту:**
```csharp
// WASM component
<InputFile OnChange="@OnFileSelected" accept=".csv" />

@code {
    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
        content.Add(fileContent, "file", file.Name);
        
        var response = await httpClient.PostAsync("/api/catalog/import", content);
        // Handle response
    }
}
```

**API endpoint:**
```csharp
app.MapPost("/api/catalog/import", async (IFormFile file, ICatalogService service) =>
{
    using var stream = file.OpenReadStream();
    var result = await service.ImportCsvAsync(stream);
    return Results.Ok(result);
});
```

**Критерії прийняття:**
- ✅ CSV імпорт працює
- ✅ Файли експортуються
- ✅ Обробка помилок файлів

## ФАЗА 8: Оптимізація та продуктивність

### Етап 8.1: Bundle optimization
**Час:** 1 тиждень

**Налаштування:**
```xml
<!-- WasmHost.csproj -->
<PropertyGroup>
  <BlazorWebAssemblyLoadAllGlobalizationData>false</BlazorWebAssemblyLoadAllGlobalizationData>
  <BlazorWebAssemblyPreserveCollationData>false</BlazorWebAssemblyPreserveCollationData>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>
  <BlazorEnableCompression>true</BlazorEnableCompression>
</PropertyGroup>
```

**Lazy loading модулів:**
```csharp
@page "/reporting"
@using Microsoft.AspNetCore.Components.Routing

<Router AppAssembly="@typeof(App).Assembly" 
        OnNavigateAsync="@OnNavigateAsync"
        AdditionalAssemblies="@lazyLoadedAssemblies">
    <!-- Router content -->
</Router>

@code {
    private List<Assembly> lazyLoadedAssemblies = new();
    
    private async Task OnNavigateAsync(NavigationContext args)
    {
        if (args.Path.StartsWith("/reporting") && !lazyLoadedAssemblies.Any(a => a.GetName().Name?.Contains("Reporting") == true))
        {
            var assemblies = await AssemblyLoadContext.Default.LoadFromAssemblyNameAsync(
                new AssemblyName("WebSearchIndexing.Modules.Reporting.Ui"));
            lazyLoadedAssemblies.Add(assemblies);
        }
    }
}
```

**Критерії прийняття:**
- ✅ Initial bundle < 10MB
- ✅ Lazy loading працює
- ✅ Компресія включена

### Етап 8.2: Кешування та Service Worker
**Час:** 3 дні

**Service Worker:**
```javascript
// wwwroot/service-worker.js
const CACHE_NAME = 'websearchindexing-v1';
const urlsToCache = [
    '/',
    '/css/app.css',
    '/WebSearchIndexing.Hosts.WasmHost.styles.css',
    '/_framework/blazor.webassembly.js'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});
```

**HTTP кешування:**
```csharp
public class CacheService
{
    private readonly Dictionary<string, (DateTime expiry, object data)> _cache = new();
    
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cached) && cached.expiry > DateTime.UtcNow)
        {
            return (T)cached.data;
        }
        return default;
    }
    
    public void Set<T>(string key, T data, TimeSpan expiry)
    {
        _cache[key] = (DateTime.UtcNow.Add(expiry), data);
    }
}
```

**Критерії прийняття:**
- ✅ Офлайн підтримка базових сторінок
- ✅ API responses кешуються
- ✅ Швидке завантаження повторних візитів

## ФАЗА 9: Розгортання та DevOps

### Етап 9.1: Налаштування розгортання
**Час:** 1 тиждень

**Docker configuration:**
```dockerfile
# ApiHost Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Hosts/ApiHost/WebSearchIndexing.Hosts.ApiHost.csproj"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebSearchIndexing.Hosts.ApiHost.dll"]
```

**WASM статичні файли:**
```dockerfile
# WASM build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Hosts/WasmHost/WebSearchIndexing.Hosts.WasmHost.csproj"
RUN dotnet publish -c Release -o /app/publish

# Static files hosting
FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
```

**Критерії прийняття:**
- ✅ API і WASM розгортаються окремо
- ✅ CORS налаштований правильно
- ✅ HTTPS працює

### Етап 9.2: CI/CD pipeline
**Час:** 3 дні

**GitHub Actions / Azure DevOps:**
```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]

jobs:
  build-api:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 9.0.x
    - name: Build API
      run: dotnet build src/Hosts/ApiHost/
    - name: Test
      run: dotnet test
    - name: Publish API
      run: dotnet publish -c Release src/Hosts/ApiHost/
      
  build-wasm:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 9.0.x
    - name: Publish WASM
      run: dotnet publish -c Release src/Hosts/WasmHost/
    - name: Deploy to CDN
      # Deploy static files to CDN
```

**Критерії прийняття:**
- ✅ Автоматичне розгортання з git
- ✅ Тести виконуються перед розгортанням
- ✅ Rollback можливий

## ФАЗА 10: Тестування та якість

### Етап 10.1: Automated testing
**Час:** 1 тиждень

**End-to-end тести:**
```csharp
// Tests/E2E/AuthenticationTests.cs
[Test]
public async Task User_CanLogin_Through_WASM()
{
    // Arrange
    await using var factory = new WebApplicationFactory<ApiHost.Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", new 
    { 
        Username = "test@example.com", 
        Password = "password" 
    });
    
    // Assert
    response.Should().BeSuccessful();
    var result = await response.Content.ReadFromJsonAsync<AuthResult>();
    result.Token.Should().NotBeNullOrEmpty();
}
```

**Component тести:**
```csharp
// Tests/Components/SettingsPageTests.cs
[Test]
public void SettingsPage_Renders_Correctly()
{
    // Arrange
    using var ctx = new TestContext();
    ctx.Services.AddMudServices();
    
    // Act
    var component = ctx.RenderComponent<SettingsPage>();
    
    // Assert
    component.Find("h5").TextContent.Should().Contain("Your settings");
}
```

**Критерії прийняття:**
- ✅ E2E тести покривають основні сценарії
- ✅ Component тести проходять
- ✅ API тести працюють

### Етап 10.2: Performance testing
**Час:** 3 дні

**Метрики для перевірки:**
- Bundle size < 10MB
- First load < 5 секунд
- Navigation < 500ms
- API response time < 200ms
- Memory usage стабільне

**Load testing:**
```csharp
// NBomber load test
var scenario = Scenario.Create("api_load_test", async context =>
{
    var response = await httpClient.GetAsync("/api/v1/core/settings");
    return Response.Ok();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(5))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
```

**Критерії прийняття:**
- ✅ Performance requirements виконані
- ✅ Немає memory leaks
- ✅ API витримує навантаження

## Ризики та мітигація

| Ризик | Ймовірність | Вплив | Мітігація |
|-------|-------------|-------|-----------|
| Великий bundle size | Високий | Середній | Lazy loading, tree shaking, compression |
| Authentication складність | Середній | Високий | Використання існуючих рішень (IdentityServer) |
| WASM performance | Середній | Середній | Profiling, optimization, caching |
| Multi-tenancy в WASM | Низький | Високий | Детальне тестування, fallback план |
| SEO втрата | Низький | Середній | Pre-rendering для публічних сторінок |

## Часова оцінка

| Фаза | Тривалість | Критичний шлях | Статус |
|------|------------|----------------|---------|
| Фаза 1: Підготовка | 1-2 тижні | Так | ✅ Завершено |
| Фаза 2: API Host | 1-2 тижні | Так | ⏳ Наступна |
| Фаза 3: UI підготовка | 2-3 тижні | Так | ✅ Завершено |
| Фаза 4: WASM Host | 2 тижні | Так | ✅ Завершено |
| Фаза 5: Аутентифікація | 2 тижні | Так | ⏸️ Очікує |
| Фаза 6: Мультитенантність | 1 тиждень | Ні | ⏸️ Очікує |
| Фаза 7: Файли | 1 тиждень | Ні |
| Фаза 8: Оптимізація | 1-2 тижні | Ні |
| Фаза 9: Розгортання | 1 тиждень | Ні |
| Фаза 10: Тестування | 1-2 тижні | Ні |

**Загальний час:** 12-19 тижнів (3-5 місяців)

## Критерії успіху

### Функціональні
- ✅ Всі поточні функції працюють в WASM
- ✅ Аутентифікація та авторизація працюють
- ✅ Мультитенантність зберігається
- ✅ Файлова функціональність працює
- ✅ Real-time оновлення (якщо потрібно)

### Нефункціональні
- ✅ Initial load time < 5 секунд
- ✅ Navigation time < 500ms
- ✅ Bundle size < 10MB
- ✅ 99.9% uptime
- ✅ Безпека на рівні поточного рішення

### Технічні
- ✅ Модульна архітектура збережена
- ✅ Clean Architecture принципи дотримані  
- ✅ Код покритий тестами (>80%)
- ✅ Documentation оновлена
- ✅ CI/CD pipeline працює

## Післямігровий план

### Видалення застарілого коду
1. Видалити WebHost проект
2. Видалити server-specific залежності з UI модулів  
3. Оновити документацію
4. Очистити невикористані пакети

### Моніторинг та аналітика
1. Application Insights / OpenTelemetry
2. Real User Monitoring (RUM)
3. Performance metrics
4. Error tracking

### Подальший розвиток
1. PWA capabilities
2. Offline functionality
3. Push notifications  
4. Advanced caching strategies

Цей план забезпечує повну міграцію з Blazor Server на Blazor WASM з збереженням всієї функціональності та можливістю верифікації на кожному етапі.
