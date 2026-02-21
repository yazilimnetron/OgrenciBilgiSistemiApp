# CLAUDE.md — OgrenciBilgiSistemi

AI assistant guide for the **OgrenciBilgiSistemi** (Student Information System) codebase.

---

## Project Overview

ASP.NET Core MVC web application (.NET 9.0) serving as a school/institution management platform. Covers students, parents, fees (aidat), cafeteria, visitors, staff, teachers, library, hardware device integration, and role-based access control.

**Platform constraint:** Windows-only. The project references the ZKTeco biometric reader COM library (`zkemkeeper`) and targets `PlatformTarget=x86`. It cannot run on Linux or macOS containers without removing or stubbing the hardware integration.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 9.0 |
| ORM | Entity Framework Core 9.0 |
| Database (dev) | SQL Server LocalDB (`MSSQLLocalDB`) |
| Database (prod) | SQL Server (connection string via env/secrets) |
| Database (alt) | SQLite (also supported via `Microsoft.EntityFrameworkCore.Sqlite`) |
| Real-time | SignalR 1.2.0 (`/kartOkuHub`) |
| Excel export | ClosedXML 0.104.2 |
| Resilience | Polly 8.5.2 |
| Serial comms | System.IO.Ports 9.0.7 |
| Hardware | ZKTeco COM interop (`zkemkeeper`, x86 Windows-only) |
| Auth | Cookie authentication (8-hour sliding session) |
| UI framework | Bootstrap 5.3.3, jQuery 3.7.1, Font Awesome 6.7.2 |
| Charts | Chart.js (via libman) |
| Client libs | libman (manages wwwroot/lib downloads) |

---

## Development Commands

```bash
# Build
dotnet build

# Run (development) — served at http://localhost:5135 or https://localhost:7045
dotnet run

# EF Core migrations (uses local tool from .config/dotnet-tools.json)
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Restore client-side libraries (Bootstrap, jQuery, Chart.js, etc.)
libman restore

# Publish
dotnet publish -c Release
```

> No test project exists in the solution. Tests are listed as a P2 roadmap item.
> There is no `README.md` in the repo — this `CLAUDE.md` is the primary reference document.

---

## Configuration

### Connection Strings

- **Development:** `appsettings.Development.json` — LocalDB (`Server=(localdb)\\MSSQLLocalDB;Database=OgrenciBilgiSistemiDb;...`)
- **Production:** Set via environment variable `ConnectionStrings__DefaultConnection` or secrets manager. Never use a machine-specific path in production — startup validation will throw if a `DESKTOP-` string is detected in production mode.

`appsettings.json` intentionally ships with an empty `DefaultConnection`; the dev override in `appsettings.Development.json` fills it.

### Application Startup (`Program.cs`)

- `AddDbContextPool<AppDbContext>` with SQL Server and retry-on-failure (5 retries, 10s max delay).
- Global `AuthorizeFilter` — all MVC endpoints require authentication by default.
- Cookie auth: login path `/Hesaplar/Giris`, access-denied path `/Hesaplar/YetkisizGiris`.
- Background hosted services started: `CardReadEventHandlerService`, `ZkConnectionMonitorHostedService`.
- SignalR hub mapped at `/kartOkuHub`.
- EF Core `Migrate()` runs on startup — schema is always up to date on launch.

---

## Directory Structure

```
OgrenciBilgiSistemiApp/
├── Controllers/             # 15 MVC controllers
├── Services/
│   ├── Interfaces/          # Service interfaces (I prefix)
│   ├── Implementations/     # Service implementations
│   └── BackgroundServices/  # IHostedService implementations
├── Models/
│   ├── *.cs                 # Domain models (...Model suffix)
│   └── Enums/               # Enum types
├── Data/
│   └── AppDbContext.cs      # EF Core DbContext
├── Migrations/              # EF Core migration files (19 migrations)
├── Views/                   # Razor views, one folder per controller
│   └── Shared/              # Layouts, partials
├── ViewModels/              # View model classes (...Vm suffix)
├── Dtos/                    # Data transfer objects (...Dto suffix)
├── Helpers/                 # Static helper classes
├── Abstractions/            # Core interfaces (e.g., IFileStorage)
├── Infrastructure/
│   └── FileStorage/         # LocalFileStorage implementation
├── Hubs/                    # SignalR hub (KartOkuHub)
├── ViewComponents/          # Razor ViewComponents (MenuViewComponent)
├── wwwroot/                 # Static files
│   ├── css/
│   ├── js/                  # home-dashboard.js, signalr-handler.js, site.js
│   ├── images/
│   ├── lib/                 # Client-side libraries (libman managed)
│   └── uploads/             # User-uploaded files (student photos etc.)
├── Properties/              # launchSettings.json (launch URLs: 5135/7045)
├── .config/
│   └── dotnet-tools.json    # Local EF Core CLI tool registration
├── appsettings.json
├── appsettings.Development.json
├── libman.json              # Client library manifest (Bootstrap, jQuery, etc.)
├── Program.cs
├── OgrenciBilgiSistemi.csproj
├── OgrenciBilgiSistemi.sln
├── AUTHORIZATION_MATRIX.md
└── UYGULAMA_ANALIZI.md
```

---

## Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Domain model | `...Model` suffix | `OgrenciModel`, `PersonelModel` |
| Service interface | `I...Service` in `Services/Interfaces/` | `IOgrenciService` |
| Service implementation | `...Service` in `Services/Implementations/` | `OgrenciService` |
| Background service | descriptive name in `Services/BackgroundServices/` | `CardReadEventHandlerService` |
| View model | `...Vm` suffix in `ViewModels/` | `OgrenciListeVm`, `AidatRaporVm` |
| DTO | `...Dto` suffix in `Dtos/` | `OgrenciAidatDto`, `DashboardStatsDto` |
| Controller | `...Controller` suffix in `Controllers/` | `OgrencilerController` |
| Hub | `...Hub` suffix in `Hubs/` | `KartOkuHub` |
| ViewComponent | `...ViewComponent` in `ViewComponents/` | `MenuViewComponent` |

All identifiers use **Turkish domain vocabulary** (see glossary below). No translation to English for domain names.

---

## Domain Terminology (Turkish → English)

| Turkish | English |
|---|---|
| Öğrenci / Ogrenci | Student |
| Veli | Parent / Guardian |
| Aidat | Dues / Subscription fee |
| Yemekhane | Cafeteria / School canteen |
| Personel | Staff member |
| Öğretmen / Ogretmen | Teacher |
| Birim | Unit / Department / Class |
| Ziyaretçi / Ziyaretci | Visitor |
| Cihaz | Hardware device |
| Hesap | Account |
| Kullanıcı / Kullanici | User |
| Menü / Menu | Navigation menu |
| Geçiş / Gecis | Entry/exit passage |
| Kart Oku | Card read (biometric) |
| Kitap | Book |
| Kitap Detay | Book loan record |
| Akademik Dönem | Academic year |
| Tarife | Rate / price schedule |
| Ödeme / Odeme | Payment |

---

## Architecture

### Layer Responsibilities

1. **Controller layer** — receives HTTP requests, builds ViewModels, orchestrates service calls, returns views or redirects.
2. **Service layer** — business logic, domain rules, complex queries, hardware/external system integration. Always injected via interface.
3. **Data layer** — `AppDbContext` + EF Core Fluent API configuration, global query filters, and relationship/constraint definitions.
4. **UI layer** — Razor Views + `MenuViewComponent` for dynamic, claim-based navigation rendering.

### Dependency Injection Lifetimes

| Service | Lifetime | Reason |
|---|---|---|
| `IZKTecoService` / `ZKTecoService` | Singleton | Hardware connection must persist across requests |
| All other `I...Service` | Scoped | Per-request EF Core operations |
| `IFileStorage` / `LocalFileStorage` | Scoped | Stateless file operations |
| Background services | Hosted (singleton) | Long-running device monitoring |

### Key Design Patterns

- **Interface-based services** — every service has an `I...Service` contract; controllers never depend on concrete types.
- **ViewModel separation** — controllers map domain models to `...Vm` types before passing to views. Raw models are not passed to views.
- **DTO separation** — service methods accept/return `...Dto` types at boundaries, not EF models.
- **Repository via EF Core** — no separate repository layer; services use `AppDbContext` directly. `AsNoTracking()` is used on read-only queries.
- **Global query filter** — `OgrenciModel` has a query filter: `o.OgrenciDurum || IncludePasifOgrenciler`. Toggle `AppDbContext.IncludePasifOgrenciler = true` to include inactive students.

---

## Authentication & Authorization

Documented in detail in `AUTHORIZATION_MATRIX.md`.

| Rule | Detail |
|---|---|
| Default | Global `AuthorizeFilter` — all endpoints require a logged-in user |
| Anonymous endpoints | Only `HesaplarController.Giris` and `HesaplarController.YetkisizGiris` via `[AllowAnonymous]` |
| Admin-only | `KullanicilarController` requires `AdminOnly` policy (`RequireRole("Admin")`) |
| All others | Any authenticated user; role-based filtering is currently enforced at UI/menu level |

The **dynamic menu** system (`MenuService` + `MenuViewComponent`) controls what each user sees based on `KullaniciMenuModel` assignments. Role-based enforcement at action level is a roadmap item (see `AUTHORIZATION_MATRIX.md`).

---

## Database

### Migrations

EF Core code-first migrations in `Migrations/`. Startup automatically calls `context.Database.Migrate()`.

```bash
# Add a new migration
dotnet ef migrations add <DescriptiveName>

# Apply manually (usually not needed — startup handles it)
dotnet ef database update
```

**Warning:** Menü seed data is embedded in migrations. On manual DB interventions be careful about ID collisions in `MenuOgeler`.

### Notable DbContext Details

- `AddDbContextPool` — connection pooling enabled.
- Retry-on-failure: 5 retries with 10-second max delay.
- Relationships configured via Fluent API (not data annotations).
- `DeleteBehavior.SetNull` used for optional foreign keys (Personel → Ogrenci, Veli → Ogrenci).
- Check constraints and unique indexes defined for Aidat, Yemekhane, tarife, and payment tables.

---

## Hardware Integration (ZKTeco)

- **COM reference:** `zkemkeeper` — Windows-only, x86 binary.
- **`ZKTecoService`** (Singleton): manages device connection, uses `SemaphoreSlim` to prevent concurrent connection races, handles COM resource release via `Marshal.FinalReleaseComObject`.
- **`CardReadEventHandlerService`** (BackgroundService): converts raw card-read events into domain entry/exit records.
- **`ZkConnectionMonitorHostedService`** (BackgroundService): monitors device connectivity and reconnects.
- **`KartOkuHub`** (SignalR): pushes card-read events to browser clients in real time.

To test without hardware, the device integration must be stubbed or the background services disabled.

---

## File Storage

- Interface: `IFileStorage` in `Abstractions/`
- Implementation: `LocalFileStorage` in `Infrastructure/FileStorage/`
- Uploaded files are stored under `wwwroot/uploads/`
- Extension and size validation is applied; MIME verification is a known gap.

---

## Excel Export

ClosedXML is used in `OgrencilerController` and `AidatService` to generate `.xlsx` reports. Reports are streamed directly to the browser response.

---

## Academic Year

`AkademikDonemHelper` (in `Helpers/`) determines the current academic year: September is the start of the academic year.

```csharp
// Returns current year if month >= 9, else year - 1
AkademikDonemHelper.Current();
AkademikDonemHelper.FromDate(someDate);
```

---

## Key Files Reference

| File | Purpose |
|---|---|
| `Program.cs` | App startup, DI registration, middleware pipeline |
| `Data/AppDbContext.cs` | EF Core context, DbSets, Fluent API config |
| `Services/Implementations/ZKTecoService.cs` | ZKTeco hardware communication |
| `Services/BackgroundServices/CardReadEventHandlerService.cs` | Card event processing |
| `Hubs/KartOkuHub.cs` | SignalR hub for real-time card reads |
| `ViewComponents/MenuViewComponent.cs` | Dynamic role-based nav menu |
| `Helpers/AkademikDonemHelper.cs` | Academic year calculation |
| `libman.json` | Client-side library manifest (Bootstrap, jQuery, Chart.js, SignalR) |
| `.config/dotnet-tools.json` | Local EF CLI tool — run `dotnet tool restore` if `dotnet ef` is missing |
| `AUTHORIZATION_MATRIX.md` | Authorization policy documentation |
| `UYGULAMA_ANALIZI.md` | Deep technical analysis and improvement roadmap |
| `appsettings.Development.json` | Local dev database connection |

---

## Known Issues & Technical Debt

See `UYGULAMA_ANALIZI.md` for the full analysis. Summary of critical items:

1. **Connection string security** — production must use environment variables / secrets manager, not `appsettings.json`.
2. **Authorization gaps** — role-based policy is enforced only at menu/UI level for most controllers; action-level enforcement is pending.
3. **Large controllers** — `OgrencilerController` mixes orchestration, file handling, report generation, and device sync; refactoring to application services is planned.
4. **Commented-out code** — several files contain large blocks of old implementation left as comments; these should be removed.
5. **Encoding** — some source files have corrupted Turkish characters (encoding artifacts); UTF-8 consistency across the repo is needed.
6. **No test project** — no automated tests exist; integration tests for auth flows and report queries are a P2 roadmap item.

---

## Making Changes

- **New domain feature:** add Model → Migration → Service interface → Service implementation → Controller → Views → ViewModel/DTO as needed. Register the service in `Program.cs`.
- **New migration:** always give it a meaningful name describing the schema change. Never edit existing migrations that have been applied.
- **New menu item:** insert into `MenuOgeler` via a migration seed or directly via the admin UI; assign to roles/users via `KullaniciMenuOgeler`.
- **Authorization:** for any new controller action that should be admin-only, add `[Authorize(Policy = "AdminOnly")]`. Update `AUTHORIZATION_MATRIX.md`.
- **Turkish naming:** follow existing domain vocabulary. Do not introduce English domain names for Turkish school concepts.
