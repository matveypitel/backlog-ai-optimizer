namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record CompetitorFeatureLlmItem(
    string Name,
    string Description,
    string? Category,
    string[] KeyBenefits,
    string[] UseCases,
    string Differentiators,
    string? TargetAudience);

internal sealed record CompetitorFeaturesWrapper(CompetitorFeatureLlmItem[] Features);
