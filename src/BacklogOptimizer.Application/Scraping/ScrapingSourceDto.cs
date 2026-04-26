namespace BacklogOptimizer.Application.Scraping;

public sealed record ScrapingSourceDto(
    Guid Id,
    string Url,
    string? Name,
    bool IsActive,
    DateTime? LastScrapedAt,
    int? RefreshIntervalHours,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateScrapingSourceCommand(string Url, string? Name, int? RefreshIntervalHours);

public sealed record UpdateScrapingSourceCommand(string? Name, bool IsActive, int? RefreshIntervalHours);

public sealed record SyncAllResult(int Enqueued, int Skipped);
