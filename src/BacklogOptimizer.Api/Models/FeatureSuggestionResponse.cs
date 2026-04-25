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
    string BusinessValue,
    string EstimatedImpact,
    string UserStories,
    string AcceptanceCriteria,
    DateTime AnalyzedAt);
