namespace BacklogOptimizer.Api.Models;

public sealed record ScrapingJobResponse(
    Guid Id,
    string Url,
    string Status,
    int AttemptCount,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
