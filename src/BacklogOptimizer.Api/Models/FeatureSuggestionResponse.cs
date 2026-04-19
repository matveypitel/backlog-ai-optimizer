namespace BacklogOptimizer.Api.Models;

public sealed record FeatureSuggestionResponse(
    Guid Id,
    string Title,
    string Description,
    string IssueType,
    string SuggestedPriority,
    string Tags,
    string Reasoning,
    string CompetitorEvidence,
    DateTime AnalyzedAt);
