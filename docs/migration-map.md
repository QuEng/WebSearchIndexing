# Мапінг «поточна фіча → цільовий модуль»

| Поточна фіча/код | Цільовий модуль | Клас/отримувач | Коментар |
| --- | --- | --- | --- |
| `src/WebSearchIndexing/Pages/HomePage*.razor` | `Modules/Core.Ui` + `Modules/Core.Application` | `DashboardPage` (UI) / `DashboardMetricsQuery` | Дашборд переїжджає до Core як частина базового досвіду керування. |
| `src/WebSearchIndexing/Pages/ServiceAccountsPage*.razor` | `Modules/Catalog.Ui` + `Modules/Catalog.Application` | `ServiceAccountsPage` / `ServiceAccountManagementService` | CRUD сервісних акаунтів стає частиною каталогу ресурсів. |
| `src/WebSearchIndexing/Pages/Dialogs/AddServiceAccountDialog*.razor` | `Modules/Catalog.Ui` | `AddServiceAccountDialog` | Діалог додавання акаунтів залишається поряд зі сторінкою каталогу. |
| `src/WebSearchIndexing/Pages/SettingsPage*.razor` | `Modules/Core.Ui` + `Modules/Core.Application` | `SettingsPage` / `TenantSettingsService` | Налаштування тенанта відносяться до Core. |
| `src/WebSearchIndexing/Pages/Urls/AllUrlsPage*.razor` + `UrlsTableComponent*.razor` | `Modules/Catalog.Ui` + `Modules/Catalog.Application` | `AllUrlsPage` / `UrlCatalogService` | CRUD, фільтри та імпорт URL залишаємо в каталозі. |
| `src/WebSearchIndexing/Pages/Urls/ProcessedUrlsPage*.razor` + `ProcessedUrlsTableComponent*.razor` | `Modules/Catalog.Ui` + `Modules/Catalog.Application` | `ProcessedUrlsPage` / `UrlProcessingHistoryQuery` | Відображення історії обробки URL-ів як частина каталогу. |
| `src/WebSearchIndexing/Pages/Urls/Components/RejectedUrlsTableComponent*.razor` | `Modules/Catalog.Ui` + `Modules/Catalog.Application` | `RejectedUrlsTable` / `UrlFailureLogQuery` | Таблиця відхилених запитів. |
| `src/WebSearchIndexing/Pages/Urls/Dialogs/LoadUrlsDialog*.razor` | `Modules/Catalog.Ui` + `Modules/Catalog.Application` | `LoadUrlsDialog` / `UrlImportService` | Логіка імпорту URL-ів у каталозі. |
| `src/WebSearchIndexing/Pages/Layout/*` | `Modules/Core.Ui` | `ShellLayout` / `ThemeProvider` | Макет, тема та доступ — частина Core UI. |
| `src/WebSearchIndexing/Pages/App.razor`, `Routes.razor`, `Error.razor` | `Hosts/WebHost` | `WebHostAppShell` | Хостова збірка контролює маршрутизацію та помилки. |
| `src/WebSearchIndexing/Pages/Components/*` | `Modules/Core.Ui` | `ComponentBase`, `NotificationPanel` | Базові UI-компоненти спільного використання. |
| `src/WebSearchIndexing/BackgroundJobs/RequestSenderWorker.cs` | `Modules/Submission.Worker` | `GoogleSubmissionWorker` | Hosted service для обробки черги. |
| `src/WebSearchIndexing/BackgroundJobs/ScopedRequestSendingService.cs` | `Modules/Submission.Application` | `UrlSubmissionOrchestrator` | Оркестрація відправки та лімітів. |
| `src/WebSearchIndexing/BackgroundJobs/RequestSender.cs` | `Modules/Submission.Infrastructure` | `GoogleIndexingClient` | Інтеграція з Google Indexing API. |
| `src/WebSearchIndexing/BackgroundJobs/IScopedRequestSendingService.cs` | `Modules/Submission.Application` | `ISubmissionProcessor` | Контракт сервісу подачі запитів. |
| `src/WebSearchIndexing/Configurations/ConfigureConnections.cs` | `Modules/Core.Infrastructure` | `DbContextFactoryConfigurator` | Постачальник EF Core підʼєднань. |
| `src/WebSearchIndexing/Configurations/ServicesConfigurations.cs` | `Modules/Catalog.Infrastructure` | `CatalogModuleServiceCollectionExtensions` | Реєстрація репозиторіїв каталогу в DI. |
| `src/WebSearchIndexing/Extensions/HostingExtensions.cs` | `Hosts/WebHost` | `WebHostStartup` | Побудова пайплайну та старту хоста. |
| `src/WebSearchIndexing/Program.cs` | `Hosts/WebHost` | `Program` | Точка входу хоста з модульною конфігурацією. |
| `src/WebSearchIndexing/Utils/EventUtil.cs` | `BuildingBlocks/Web` | `EventDispatcher` | Загальна утиліта подій стає частиною building blocks. |
| `src/WebSearchIndexing.Data/IndexingDbContext.cs` | `Modules/Catalog.Infrastructure` | `CatalogDbContext` | Контекст для каталогу URL-ів та сервісних акаунтів. |
| `src/WebSearchIndexing.Data/Repositories/ServiceAccountRepository.cs` | `Modules/Catalog.Infrastructure` | `ServiceAccountRepository` | Реалізація репозиторію каталогу. |
| `src/WebSearchIndexing.Data/Repositories/UrlRequestRepository.cs` | `Modules/Catalog.Infrastructure` | `UrlRequestRepository` | Реалізація репозиторію запитів. |
| `src/WebSearchIndexing.Data/Repositories/SettingRepository.cs` | `Modules/Core.Infrastructure` | `TenantSettingRepository` | Налаштування переміщуються до Core. |
| `src/WebSearchIndexing.Domain/Entities/ServiceAccount.cs` | `Modules/Catalog.Domain` | `ServiceAccountAggregate` | Доменна модель каталогу. |
| `src/WebSearchIndexing.Domain/Entities/UrlRequest.cs` | `Modules/Catalog.Domain` | `UrlRequestAggregate` | Домен запитів каталогу. |
| `src/WebSearchIndexing.Domain/Entities/Setting.cs` | `Modules/Core.Domain` | `TenantSettingAggregate` | Доменна модель Core. |
| `src/WebSearchIndexing.Domain/Repositories/IServiceAccountRepository.cs` | `Modules/Catalog.Domain` | `IServiceAccountRepository` | Контракт каталогу. |
| `src/WebSearchIndexing.Domain/Repositories/IUrlRequestRepository.cs` | `Modules/Catalog.Domain` | `IUrlCatalogRepository` | Контракт каталогу URL-ів. |
| `src/WebSearchIndexing.Domain/Repositories/ISettingRepository.cs` | `Modules/Core.Domain` | `ITenantSettingRepository` | Контракт Core. |
| `src/WebSearchIndexing.Domain/Repositories/IRepository.cs` | `BuildingBlocks/Persistence` | `IAggregateRepository<T>` | Базовий контракт у building blocks. |
