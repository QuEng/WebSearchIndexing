# WebSearchIndexing Migration Map

| Old type | Target module | Target layer | Notes |
| --- | --- | --- | --- |
| `WebSearchIndexing.Extensions.HostingExtensions` | `Hosts.WebHost` | `Ui` | Split between host `ServiceCollectionExtensions` and `ApplicationBuilderExtensions` invoked by `Program`. |
| `WebSearchIndexing.Configurations.ServicesConfigurations` | `Modules.Catalog` | `Infrastructure` | Register Catalog repositories via a module-level DI extension in infrastructure. |
| `WebSearchIndexing.Configurations.ConfigureConnections` | `Hosts.WebHost` | `Infrastructure` | Fold into the host `AddInfrastructure` pipeline that wires pooled `CatalogDbContext` and `CoreDbContext` factories. |
| `WebSearchIndexing.Utils.EventUtil` | `BuildingBlocks` | `Ui` | Relocate to `BuildingBlocks.Web` as a shared Blazor event helper. |
| `WebSearchIndexing.Graphics.Icon` | `BuildingBlocks` | `Ui` | Move static icon placeholders (including nested `Filled/Outlined/Background`) into shared UI utilities. |
| `WebSearchIndexing.Theming.ThemeColor` | `Hosts.WebHost` | `Ui` | Use the host-level theming definitions already under `Hosts/WebHost/Theming`. |
| `WebSearchIndexing.Theming.ThemeColorExtensions` | `Hosts.WebHost` | `Ui` | Consolidate with the existing host color extension helpers. |
| `WebSearchIndexing.Theming.GlobalTheme` | `Hosts.WebHost` | `Ui` | Reuse the host `GlobalTheme` implementation and retire the duplicate. |
| `WebSearchIndexing.Theming.PaletteLightCustom` | `Hosts.WebHost` | `Ui` | Merge with the host light palette configuration. |
| `WebSearchIndexing.Theming.PaletteDarkCustom` | `Hosts.WebHost` | `Ui` | Merge with the host dark palette configuration. |
| `WebSearchIndexing.Theming.TypographyCustom` | `Hosts.WebHost` | `Ui` | Align with the host typography customization. |
| `WebSearchIndexing.Theming.LayoutPropertiesCustom` | `Hosts.WebHost` | `Ui` | Align with the host layout properties customization. |
| `WebSearchIndexing.Theming.IPaletteCustom` | `Hosts.WebHost` | `Ui` | Keep the palette contract alongside the host theming assets. |
| `WebSearchIndexing.Pages.Components.ComponentBase` | `BuildingBlocks` | `Ui` | Replace with `BuildingBlocks.Web.Components.ComponentBase`. |
| `WebSearchIndexing.Pages.Components.DataPairComponent<T>` | `Modules.Core` | `Ui` | Port as a Core UI component for the dashboard metric cards. |
| `WebSearchIndexing.Pages.Components.Notification.NotificationType` | `BuildingBlocks` | `Ui` | Share notification type enum via building-blocks UI utilities. |
| `WebSearchIndexing.Pages.Components.Notification.BaseNotificationComponent` | `BuildingBlocks` | `Ui` | Move notification wrapper component to building-blocks for reuse. |
| `WebSearchIndexing.Pages.HomePage` | `Modules.Core` | `Ui` | Port dashboard landing page into the Core UI module. |
| `WebSearchIndexing.Pages.SettingsPage` | `Modules.Core` | `Ui` | Superseded by `Modules.Core.Ui.Pages.Settings.SettingsPage`. |
| `WebSearchIndexing.Pages.ServiceAccountsPage` | `Modules.Catalog` | `Ui` | Superseded by `Modules.Catalog.Ui.Pages.ServiceAccounts.ServiceAccountsPage`. |
| `WebSearchIndexing.Pages.Dialogs.AddServiceAccountDialog` | `Modules.Catalog` | `Ui` | Superseded by the Catalog UI dialog under `Pages/ServiceAccounts/Dialogs`. |
| `WebSearchIndexing.Pages.Urls.AllUrlsPage` | `Modules.Catalog` | `Ui` | Superseded by `Modules.Catalog.Ui.Pages.Urls.AllUrlsPage`. |
| `WebSearchIndexing.Pages.Urls.ProcessedUrlsPage` | `Modules.Catalog` | `Ui` | Superseded by `Modules.Catalog.Ui.Pages.Urls.ProcessedUrlsPage`. |
| `WebSearchIndexing.Pages.Urls.Components.UrlsTableComponent` | `Modules.Catalog` | `Ui` | Superseded by the Catalog UI component under `Pages/Urls/Components`. |
| `WebSearchIndexing.Pages.Urls.Components.ProcessedUrlsTableComponent` | `Modules.Catalog` | `Ui` | Superseded by the Catalog UI component under `Pages/Urls/Components`. |
| `WebSearchIndexing.Pages.Urls.Components.RejectedUrlsTableComponent` | `Modules.Catalog` | `Ui` | Superseded by the Catalog UI component under `Pages/Urls/Components`. |
| `WebSearchIndexing.Pages.Urls.Dialogs.LoadUrlsDialog` | `Modules.Catalog` | `Ui` | Superseded by the Catalog UI dialog under `Pages/Urls/Dialogs`. |
| `WebSearchIndexing.Pages.Layout.MainLayout` | `Hosts.WebHost` | `Ui` | Replace with `Hosts.WebHost.Components.Layout.MainLayout`. |
| `WebSearchIndexing.Pages.Layout.CustomThemeProvider` | `Hosts.WebHost` | `Ui` | Replace with `Hosts.WebHost.Components.Layout.CustomThemeProvider`. |
| `WebSearchIndexing.Pages.Layout.Components.AccessComponent` | `Modules.Core` | `Ui` | Superseded by `Modules.Core.Ui.Components.AccessComponent`. |
| `WebSearchIndexing.Data.IndexingDbContext` | `Modules.Catalog` | `Infrastructure` | Rename to `CatalogDbContext` within `Modules.Catalog.Infrastructure.Persistence`. |
| `WebSearchIndexing.Data.Repositories.BaseRepository<T, TKey>` | `Modules.Catalog` | `Infrastructure` | Move into Catalog persistence as the base EF repository (or replace with module-specific pattern). |
| `WebSearchIndexing.Data.Repositories.ServiceAccountRepository` | `Modules.Catalog` | `Infrastructure` | Port to Catalog persistence layer alongside new `CatalogDbContext`. |
| `WebSearchIndexing.Data.Repositories.UrlRequestRepository` | `Modules.Catalog` | `Infrastructure` | Port to Catalog persistence layer alongside new `CatalogDbContext`. |
| `WebSearchIndexing.Data.Migrations.Init` | `Modules.Catalog` | `Infrastructure` | Rescope the migration set to `Modules.Catalog.Infrastructure.Persistence.Migrations`. |
| `WebSearchIndexing.Data.Migrations.IndexingDbContextModelSnapshot` | `Modules.Catalog` | `Infrastructure` | Move snapshot to the Catalog infrastructure migrations folder. |
| `WebSearchIndexing.Domain.Entities.BaseEntity<T>` | `BuildingBlocks` | `Domain` | Relocate to building-blocks domain abstractions (or drop in favour of direct `IEntity` usage). |
| `WebSearchIndexing.Domain.Repositories.IRepository<T, TKey>` | `BuildingBlocks` | `Application` | Move generic repository contract into shared application abstractions. |
| `WebSearchIndexing.Domain.Repositories.IServiceAccountRepository` | `Modules.Catalog` | `Application` | Relocate to Catalog application abstractions; wire into handlers. |
| `WebSearchIndexing.Domain.Repositories.IUrlRequestRepository` | `Modules.Catalog` | `Application` | Relocate to Catalog application abstractions; wire into handlers. |
