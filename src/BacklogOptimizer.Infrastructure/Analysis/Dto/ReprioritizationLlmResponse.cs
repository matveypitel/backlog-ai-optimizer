namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record ReprioritizationLlmResponse(
    string? SuggestedPriority,
    string Reasoning,
    string CompetitorEvidence,
    double ConfidenceScore);
