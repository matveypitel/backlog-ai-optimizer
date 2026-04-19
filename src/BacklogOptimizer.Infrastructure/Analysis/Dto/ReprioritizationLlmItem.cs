namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record ReprioritizationLlmItem(
    string JiraKey,
    string? SuggestedPriority,
    string Reasoning,
    string CompetitorEvidence,
    double ConfidenceScore);
