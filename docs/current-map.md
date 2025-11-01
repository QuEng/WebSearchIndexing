# Поточна карта проєкту «WebSearchIndexing»

## Проєкти та основні папки
- `src/WebSearchIndexing` — Blazor Server UI, DI-конфігурація та Razor-сторінки.
- `src/WebSearchIndexing.Data` — EF Core `IndexingDbContext` та реалізації репозиторіїв.
- `src/WebSearchIndexing.Domain` — доменні моделі й контракти репозиторіїв.
- `src/Modules/*` — заготовки майбутніх модулів (поки лише `AssemblyMarker`/`ModuleRegistration`).

## UI та сторінки (Razor)
- Кореневі компоненти: `Pages/App.razor`, `Pages/Routes.razor`, `Pages/Error.razor`.
- Макет і тема: `Pages/Layout/MainLayout.razor[.cs/.css]`, `Pages/Layout/CustomThemeProvider.razor[.cs]`, `Pages/Layout/Components/AccessComponent`.
- Головні сторінки:
  - `Pages/HomePage` — дашборд, читає статистику з `IServiceAccountRepository`, `IUrlRequestRepository`, `ISettingRepository`.
  - `Pages/ServiceAccountsPage` — CRUD сервісних акаунтів, використовує діалог `Pages/Dialogs/AddServiceAccountDialog`.
  - `Pages/SettingsPage` — конфігурація лімітів, запускає `IScopedRequestSendingService` при активації сервісу.
- Сторінки URL-ів: `Pages/Urls/AllUrlsPage`, `Pages/Urls/ProcessedUrlsPage` з табличними компонентами
  (`UrlsTableComponent`, `ProcessedUrlsTableComponent`, `RejectedUrlsTableComponent`) і діалогом завантаження `LoadUrlsDialog`.
- Допоміжні компоненти: `Pages/Components/*` (базовий клас, парові поля, нотифікації).

## Сервісні класи та фонові завдання
- `BackgroundJobs/RequestSenderWorker` — `BackgroundService`, створює scope та викликає `IScopedRequestSendingService`.
- `BackgroundJobs/ScopedRequestSendingService` — рівень бізнес-логіки відправки, оперує репозиторіями URL-ів, налаштувань, сервісних акаунтів.
- `BackgroundJobs/RequestSender` — статичний клієнт Google Indexing API (працює з `ServiceAccount` + `UrlRequest`).
- `Configurations/ConfigureConnections` — налаштування пулу `DbContext` (PostgreSQL + міграції).
- `Configurations/ServicesConfigurations` — DI-зв’язування інтерфейсів домену з реалізаціями з `Data`.
- `Extensions/HostingExtensions` — складання пайплайну, DI MudBlazor, запуск фонового воркера та авто-міграції.
- `Utils/EventUtil` — дрібні утиліти (делегати, broadcast подій у UI).

## Доступ до БД
- `IndexingDbContext` (EF Core) — `DbSet<ServiceAccount>`, `DbSet<UrlRequest>`, `DbSet<Setting>`.
- Репозиторії (`WebSearchIndexing.Data/Repositories`):
  - `ServiceAccountRepository`, `UrlRequestRepository`, `SettingRepository` — реалізації контрактів з домену.
  - `BaseRepository<T>` — спільна CRUD-логіка, працює через `IDbContextFactory<IndexingDbContext>`.
- Міграції зберігаються в `WebSearchIndexing.Data/Migrations`.

## Моделі (Domain)
- `Entities/ServiceAccount` — ключі Google API (квоти, JSON, timestamps).
- `Entities/UrlRequest` — черга URL-ів (тип запиту, пріоритет, статус, зв’язок із сервісним акаунтом).
- `Entities/Setting` — глобальні налаштування (ліміт запитів, прапор увімкнення).
- Репозиторні інтерфейси: `IServiceAccountRepository`, `IUrlRequestRepository`, `ISettingRepository`, базовий `IRepository`.

## Потік залежностей (хто кого викликає)
```
UI (Razor сторінки в src/WebSearchIndexing/Pages)
  -> DI інтерфейси домену (IServiceAccountRepository / IUrlRequestRepository / ISettingRepository)
    -> Реалізації з src/WebSearchIndexing.Data/Repositories
      -> IndexingDbContext (EF Core) -> PostgreSQL

BackgroundJobs/RequestSenderWorker
  -> ScopedRequestSendingService
    -> (ті самі репозиторії) -> IndexingDbContext
      -> RequestSender -> Google Indexing API

SettingsPage (в UI)
  -> IScopedRequestSendingService
    -> ScopedRequestSendingService (повторно тригерить DoWork)
```
