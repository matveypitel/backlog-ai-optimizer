namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record FeatureSuggestionLlmItem(
    string Title,
    string Description,
    string IssueType,
    string SuggestedPriority,
    string[] Tags,
    string Reasoning,
    string CompetitorEvidence,
    string BusinessValue,
    string EstimatedImpact,
    string[] UserStories,
    string[] AcceptanceCriteria);

internal sealed record FeatureSuggestionsWrapper(FeatureSuggestionLlmItem[] Suggestions);
