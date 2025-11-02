# Дорожня карта розробки (step-by-step)

Нижче — поетапні завдання у форматі чеклістів. Фокус: безпека, мультитенантність, надійність (outbox), спостережуваність, вирівнювання UI/Api між модулями та підготовка воркерів.

---

## Етап0 — Швидкі покращення видимості
- [ ] Увімкнути HealthChecks readiness endpoint (`/health/ready`) і деталізовані перевірки: БД (Catalog/Core), зовнішні сервіси при наявності.
- [ ] Додати Serilog з sink для консолі та (опційно) Seq.
- [ ] Додати OpenTelemetry: traces + metrics + logs; експортер (OTLP/console). 
- [ ] Провести базову інвентаризацію логів у `RequestSender/ScopedRequestSendingService` та воркерах.

Artifacts/зміни:
- [ ] `src/Hosts/WebHost/Program.cs` — підключення Serilog, OTEL, HealthChecks.
- [ ] `src/BuildingBlocks/Observability/DependencyInjectionExtensions.cs` — реальна реєстрація OTEL.

---

## Етап1 — Безпека секретів та доступу
- [ ] Шифрування `ServiceAccount.CredentialsJson` (Data Protection або провайдер секретів). 
- [ ] Міграція схеми: збереження ciphertext + можливість оберненого розшифрування при відправці.
- [ ] Видалити/обмежити тимчасовий `AccessComponent` або замінити на OIDC/рольову модель (Admin UI).
- [ ] Валідація вхідних даних API: мінімальні guard-и, обмеження розмірів запитів.

Artifacts/зміни:
- [ ] Catalog.Infrastructure: value converter або `SaveChanges` інтерсептор для шифрування/дешифрування.
- [ ] Конфіг `appsettings`/UserSecrets для ключів шифрування.

---

## Етап2 — Мультитенантність (вирівнювання)
- [ ] Перейти на пер-орендаторський connection string (Finbuckle store/provider) або задокументувати single-DB стратегію.
- [ ] Перевірити всі ентіті на наявність `TenantId` та глобальних фільтрів (в т.ч. майбутні таблиці Reporting/Outbox).
- [ ] Перевірити виставлення `TenantId` на вставках (інтерсептор/`SaveChanges`).

Artifacts/зміни:
- [ ] `src/Hosts/WebHost/Program.cs` — джерело ConnectionString per-tenant.
- [ ] `CatalogDbContext/CoreDbContext` — інтерсептори/виставлення `TenantId`.

---

## Етап3 — Outbox pattern і події
- [ ] Додати `BuildingBlocks.Messaging`: контракти інтеграційних подій, outbox entity + репозиторій.
- [ ] У Catalog/Core — публікація подій у межах транзакції (запис у Outbox).
- [ ] Фоновий процес-діспатчер Outbox (веб-хост або окремий воркер) з ретраями/ідемпотентністю.
- [ ] Визначити мінімальний набір подій: `ServiceAccountCreated/Updated/Deleted`, `UrlItemsImported`, `UrlItemStatusChanged`, `SettingsChanged`.

Artifacts/зміни:
- [ ] Схема БД: таблиця `outbox_messages` (+ TenantId, індекси по статусах/датах).
- [ ] `BuildingBlocks.Messaging` — DI, серіалізація подій, політики ретраю.

---

## Етап4 — Reporting як справжній модуль
- [ ] Створити `Reporting.Application` (DTO/Handlers/Queries) і `Reporting.Api` (мінімальні ендпоїнти агрегатів/статистики).
- [ ] `Reporting.Ui.Dashboard` перевести на запит до `Reporting.Api`, не читати напряму репозиторії Catalog/Core.
- [ ] Агрегації сьогодні/за період (pending/completed/failed, по типах Updated/Deleted, квоти).

Artifacts/зміни:
- [ ] `src/Modules/Reporting/Application/*`, `src/Modules/Reporting/Api/*` — реалізація запитів/ендпоїнтів.
- [ ] `DashboardPage` — споживає `Reporting.Api` через HttpClient.

---

## Етап5 — Воркери: логіка, стабільність, метрики
- [ ] `Submission.Worker` — сцена приймання/валідації батчів URL-ів (якщо не робиться у WebHost), постановка в чергу.
- [ ] `Crawler.Worker` — підготовка/верифікація URL-ів (за потреби), маркування.
- [ ] `Inspection.Worker` — валідація статусів, повторні спроби, аналіз помилок.
- [ ] Політики retry/backoff (Polly) для зовнішніх викликів.
- [ ] Експонування метрик (кількість оброблених, помилки, латентність), логи з кореляцією.

Artifacts/зміни:
- [ ] DI у відповідних `Program.cs`, впровадження сервісів/репозиторіїв.
- [ ] OpenTelemetry + Serilog у воркерах.

---

## Етап6 — API твердість та обмеження
- [ ] Увімкнути `RateLimiter` для `/api/*` (політики: фіксоване вікно/токен бакет).
- [ ] Додати Swagger/OpenAPI (генерація клієнтів опційно).
- [ ] Консистентна обробка помилок (ProblemDetails), кореляційні ідентифікатори.
- [ ] Версіонування API для майбутньої еволюції.

Artifacts/зміни:
- [ ] `src/Hosts/WebHost/Program.cs` — `AddRateLimiter`, `UseRateLimiter`, Swagger.
- [ ] Узгодження відповідей у мінімальних ендпоїнтах модулів.

---

## Етап7 — Модель даних та продуктивність
- [ ] Перевірити індекси: `TenantId`, статус/дата для UrlItems/Outbox, унікальні ключі (Sites host, Settings key).
- [ ] Переглянути репозиторії на предмет зайвих запитів/`AsNoTracking`/пагінації.
- [ ] Кешування довідкових даних (за потреби).

---

## Етап8 — Навігація/UX у хості
- [ ] Заповнити `WebHostNavigationContributor` (групи/порядок верхнього рівня).
- [ ] Узгодити стилі/тему; привести `wwwroot/scss` і змінні теми до одного джерела правди.

---

## Етап9 — Тестування і CI/CD
- [ ] Інтеграційні тести для мінімальних API (Catalog/Core/Reporting).
- [ ] Контракти подій (snapshot-тести серіалізації) і outbox dispatcher unit-тести.
- [ ] Базовий pipeline (Build+Test), далі — релізні артефакти (контейнери).

---

## Етап10 — Документація та операційка
- [ ] Документація по розгортанню/конфігурації (секрети, OTEL, Serilog, tenants).
- [ ] Runbook для воркерів: політики ретраю/відмови, моніторинг, алерти.
- [ ] Оновити `docs/current-map.md` відповідно до актуальної архітектури.

---

## Пакети/інструменти (орієнтири)
- Serilog: `Serilog.AspNetCore`, `Serilog.Sinks.Console`, опційно `Serilog.Sinks.Seq`.
- OpenTelemetry: `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Instrumentation.AspNetCore/HttpClient/Runtime`.
- Rate Limiting: `Microsoft.AspNetCore.RateLimiting`.
- Polly: `Polly.Extensions.Http`.
- Шифрування: `Microsoft.AspNetCore.DataProtection` (або обраний секрет-менеджер).

## Definition of Done (загальне)
- Всі етапи постачають інкрементальні, перевірені зміни (лінт, тести, міграції).
- Документація й конфіг шаблони оновлені.
- Метрики/логи/трейси видимі в локальному стенді.
