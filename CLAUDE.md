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
- **Infrastructure** — EF Core, Playwright, Jira HTTP client, background services. Implements Application interfaces.
- **Api** — ASP.NET Core controllers and middleware. Depends on Application and Infrastructure.

### Entities

- **ScrapingJob** — URL to scrape, status (`Pending`/`Running`/`Completed`/`Failed`), attempt count, error message.
- **ScrapedPage** — cached page result (URL, raw HTML, title, extracted text).
- **JiraIssue** — synced Jira issue (JiraKey, summary, description, status, priority, assignee, type, Jira timestamps). Unique constraint on `JiraKey`.
- **BaseEntity** — `Guid Id`, `CreatedAt`, `UpdatedAt` (auto-populated by `SaveChangesAsync`; entities never set these).

### Scraping flow

`POST /api/scraping` → `ScrapingController` → `IScrapingService.EnqueueScrapeAsync` → `ScrapingJobService` writes a `ScrapingJob` (status=`Pending`) to the DB → returns `202 Accepted`.

`ScrapingWorker` (hosted service) polls `ScrapingJobs` every N seconds for `Pending` jobs, marks the job `Running`, runs Playwright headless browser to scrape the URL, upserts into `ScrapedPages`, then marks the job `Completed` or `Failed`. Retries use exponential backoff (`5000 * 2^attemptCount` ms). On startup, any `Running` jobs left from a previous crash are reset to `Failed`.

### Jira sync flow

`POST /api/sync` → `SyncController` → `IJiraSyncService.SyncAsync` → `JiraSyncService` calls `JiraApiClient` (Jira REST API `/rest/api/3/search/jql` with Basic auth) paginating via `nextPageToken`, upserts each issue into `JiraIssues` by `JiraKey`, collects per-issue errors (rolls back via `ChangeTracker.Clear()` on failure to allow the sync to continue) → returns `{ syncedCount, errors[] }`.

### Database

PostgreSQL via Npgsql. `ApplicationDbContext` enforces UTC on all `DateTime` columns (`timestamp with time zone`) via an `OnModelCreating` convention, and sets `CreatedAt`/`UpdatedAt` automatically in `SaveChangesAsync`.

### Configuration (`appsettings.json`)

| Section | Key | Purpose |
|---|---|---|
| `ConnectionStrings` | `DefaultConnection` | PostgreSQL connection string |
| `ScrapingWorker` | `PollingIntervalSeconds` | How often the worker polls for pending jobs (default 5) |
| `Scraping` | `Headless`, `Timeout`, `MaxRetryAttempts`, `MaxRetryDelay` | Playwright options and retry policy |
| `Jira` | `BaseUrl`, `Email`, `ApiToken` | Jira Basic auth credentials |
| `Jira` | `ProjectKey` | Required — the project to sync |
| `Jira` | `JqlFilter` | Optional override JQL; defaults to all standard issue types ordered by created |
| `Jira` | `MaxResultsPerPage` | Page size for Jira API calls (default 50) |

### HTTP client setup

`JiraApiClient` is registered as a typed `HttpClient`. Basic auth (`Email:ApiToken`, base64-encoded) and `Accept: application/json` are set in `DependencyInjection` — the client itself just makes requests.

### Middleware & API docs

- `GlobalExceptionHandler` returns `ProblemDetails` on unhandled exceptions.
- OpenAPI/Scalar UI is enabled in `Development` only.

## Code style

Follow standard Microsoft C#/.NET conventions.

- One type per file — every class, interface, enum, and record gets its own `.cs` file named after the type.
- Prefer simple, readable code over clever one-liners. If a future reader has to pause to parse it, rewrite it.
- No unnecessary abstractions. Don't introduce interfaces, base classes, or helpers until there is a concrete second use for them.
- No defensive code for impossible cases. Don't validate inputs that are guaranteed valid by the caller or the framework.
