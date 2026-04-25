namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record CompetitorFeatureLlmItem(
    string Name,
    string Description,
    string? Category);

internal sealed record CompetitorFeaturesWrapper(CompetitorFeatureLlmItem[] Features);
