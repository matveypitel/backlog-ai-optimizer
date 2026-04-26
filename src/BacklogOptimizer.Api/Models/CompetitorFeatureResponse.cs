namespace BacklogOptimizer.Api.Models;

public sealed record CompetitorFeatureResponse(
    Guid Id,
    string Name,
    string Description,
    string? Category,
    string KeyBenefits,
    string UseCases,
    string Differentiators,
    string? TargetAudience,
    string SourceUrl,
    string? SourceTitle,
    DateTime ExtractedAt);
