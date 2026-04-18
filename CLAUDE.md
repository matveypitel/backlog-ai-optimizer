# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run API
dotnet run --project src/BacklogOptimizer.Api

# Run tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Add EF migration
dotnet ef migrations add <MigrationName> --project src/BacklogOptimizer.Infrastructure --startup-project src/BacklogOptimizer.Api

# Apply migrations
dotnet ef database update --project src/BacklogOptimizer.Infrastructure --startup-project src/BacklogOptimizer.Api
```

Package versions are managed centrally in `Directory.Packages.props`. Add a `<PackageVersion>` entry there, then a `<PackageReference>` (without version) in the project file.

## Architecture

Clean Architecture with four projects:

- **Core** — domain entities and shared primitives (`Result<T>`, `Error`, `PagedResult<T>`). No dependencies on other projects.
- **Application** — interfaces and settings. Depends only on Core.
- **Infrastructure** — EF Core, Playwright, background services. Implements Application interfaces.
- **Api** — ASP.NET Core controllers and middleware. Depends on Application and Infrastructure.

### Scraping flow

`POST /api/scraping` → `ScrapingController` → `IScrapingService.EnqueueScrapeAsync` → `ScrapingJobService` writes a `ScrapingJob` (status=`Pending`) to the DB → returns `202 Accepted`.

`ScrapingWorker` (hosted service) polls the `ScrapingJobs` table every N seconds for `Pending` jobs, marks the job `Running`, runs Playwright to scrape the URL, upserts into `ScrapedPages`, then marks the job `Completed` or `Failed`. On startup it resets any `Running` jobs left over from a previous crash back to `Failed`.

### Database

PostgreSQL via Npgsql. `ApplicationDbContext` enforces UTC on all `DateTime` columns (`timestamp with time zone`) via an `OnModelCreating` convention, and sets `CreatedAt`/`UpdatedAt` automatically in `SaveChangesAsync` — entities do not manage these fields themselves.

Polling interval and Playwright options are configured in `appsettings.json` under `ScrapingWorker` and `Scraping` sections.

## Code style

Follow standard Microsoft C#/.NET conventions.

- One type per file — every class, interface, enum, and record gets its own `.cs` file named after the type.
- Prefer simple, readable code over clever one-liners. If a future reader has to pause to parse it, rewrite it.
- No unnecessary abstractions. Don't introduce interfaces, base classes, or helpers until there is a concrete second use for them.
- No defensive code for impossible cases. Don't validate inputs that are guaranteed valid by the caller or the framework.
