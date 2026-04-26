namespace BacklogOptimizer.Api.Models;

public sealed record ScrapedPageResponse(
    Guid Id,
    string Url,
    string? Title,
    int FeaturesCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
