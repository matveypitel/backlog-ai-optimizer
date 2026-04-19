namespace BacklogOptimizer.Api.Models;

public sealed record ReprioritizationSuggestionResponse(
    string JiraKey,
    string CurrentPriority,
    string SuggestedPriority,
    string Reasoning,
    string CompetitorEvidence,
    double ConfidenceScore,
    DateTime AnalyzedAt);
